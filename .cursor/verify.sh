#!/usr/bin/env bash
#
# Environment smoke test for the Impact Rush Unity 6 project.
#
# Compiles every project assembly (matching the .asmdef layout and its
# reference graph) against the Unity reference assemblies that ship with the
# installed Editor. This validates that the toolchain + engine references are
# correctly installed WITHOUT needing an activated Unity license.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_INSTALL_DIR="${UNITY_INSTALL_DIR:-/opt/unity}"
DOTNET_ROOT="${DOTNET_ROOT:-/opt/dotnet}"
DOTNET="${DOTNET_ROOT}/dotnet"
[ -x "$DOTNET" ] || DOTNET="$(command -v dotnet)"

MANAGED="${UNITY_INSTALL_DIR}/Editor/Data/Managed"
UENG_DIR="${MANAGED}/UnityEngine"
if [ ! -d "$UENG_DIR" ]; then
  echo "ERROR: Unity managed assemblies not found under $UENG_DIR" >&2
  echo "Run .cursor/install.sh first." >&2
  exit 1
fi

WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT

# Common <Reference> block: every UnityEngine module DLL.
UENG_REFS=""
for dll in "$UENG_DIR"/*.dll; do
  name="$(basename "$dll" .dll)"
  UENG_REFS+="    <Reference Include=\"${name}\"><HintPath>${dll}</HintPath><Private>false</Private></Reference>"$'\n'
done

# Editor-only references.
#
# In Unity 6 the UnityEditor.*Module.dll assemblies (which define MenuItem,
# EditorBuildSettings, etc.) already live inside the UnityEngine/ folder and are
# therefore part of $UENG_REFS. The standalone Managed/UnityEditor.dll is a
# type-forwarding facade for those same types, so referencing it as well causes
# CS0433 "type exists in both" ambiguity. We intentionally rely only on the
# module assemblies and add no extra editor references here.
UEDIT_REFS=""

# Emit a csproj for one assembly.
# args: <AssemblyName> <SourceDir> <extra-references-block> <project-references...>
emit_csproj() {
  local name="$1" srcdir="$2" extra="$3"; shift 3
  local projrefs=""
  for dep in "$@"; do
    projrefs+="    <ProjectReference Include=\"${WORK}/${dep}/${dep}.csproj\" />"$'\n'
  done
  mkdir -p "${WORK}/${name}"
  cat > "${WORK}/${name}/${name}.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>disable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <AssemblyName>${name}</AssemblyName>
    <DefineConstants>UNITY_EDITOR;UNITY_2023_1_OR_NEWER;UNITY_6000_0_OR_NEWER</DefineConstants>
    <NoWarn>CS0436;CS0618;CS0649</NoWarn>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="${srcdir}/**/*.cs" />
  </ItemGroup>
  <ItemGroup>
${UENG_REFS}${extra}${projrefs}  </ItemGroup>
</Project>
EOF
}

S="${REPO_ROOT}/Assets/Scripts"
# Dependency order mirrors the .asmdef reference graph.
emit_csproj ImpactRush.Utilities "${S}/Utilities" ""
emit_csproj ImpactRush.Core      "${S}/Core"      "" ImpactRush.Utilities
emit_csproj ImpactRush.Physics   "${S}/Physics"   "" ImpactRush.Core ImpactRush.Utilities
emit_csproj ImpactRush.Gameplay  "${S}/Gameplay"  "" ImpactRush.Core ImpactRush.Physics ImpactRush.Utilities
emit_csproj ImpactRush.UI        "${S}/UI"        "" ImpactRush.Core ImpactRush.Utilities
emit_csproj ImpactRush.Editor    "${S}/Editor"    "${UEDIT_REFS}" \
  ImpactRush.Core ImpactRush.Gameplay ImpactRush.Physics ImpactRush.UI ImpactRush.Utilities

echo "==> Compiling Impact Rush assemblies against Unity ${UNITY_INSTALL_DIR}"
rc=0
for proj in ImpactRush.Utilities ImpactRush.Core ImpactRush.Physics \
            ImpactRush.Gameplay ImpactRush.UI ImpactRush.Editor; do
  if "$DOTNET" build "${WORK}/${proj}/${proj}.csproj" -c Release -v quiet -nologo \
        /p:GenerateAssemblyInfo=false >/dev/null 2>"${WORK}/${proj}.err"; then
    echo "  PASS  ${proj}.dll"
  else
    echo "  FAIL  ${proj}"
    cat "${WORK}/${proj}.err" >&2
    rc=1
  fi
done

if [ "$rc" -eq 0 ]; then
  echo "==> SUCCESS: all 6 assemblies compiled against Unity 6 reference assemblies."
else
  echo "==> FAILURE: one or more assemblies failed to compile." >&2
fi
exit "$rc"
