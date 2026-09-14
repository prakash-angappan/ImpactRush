using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ImpactRush.Editor
{
    /// <summary>
    /// Generates simple synthesized placeholder audio clips (short sine tones) and imports them
    /// as project assets, so the audio library can be populated before real assets exist.
    /// <para>
    /// RECONSTRUCTED FILE: called by <c>UIFrameworkBuilder</c> via the seven <c>CreateOrLoad*</c>
    /// factory methods (each returning an <see cref="AudioClip"/>) but never committed to git. The
    /// method set and signatures match those call sites exactly; the synthesis is a best-effort
    /// placeholder that can be replaced with real audio later.
    /// </para>
    /// </summary>
    public static class PlaceholderAudioGenerator
    {
        private const int SampleRate = 44100;

        public static AudioClip CreateOrLoadBackgroundMusic(string assetPath) => CreateOrLoad(assetPath, 2.0f, 220f, 0.25f);

        public static AudioClip CreateOrLoadButtonClick(string assetPath) => CreateOrLoad(assetPath, 0.10f, 880f, 0.4f);

        public static AudioClip CreateOrLoadProjectileFire(string assetPath) => CreateOrLoad(assetPath, 0.20f, 320f, 0.4f);

        public static AudioClip CreateOrLoadImpact(string assetPath) => CreateOrLoad(assetPath, 0.25f, 140f, 0.5f);

        public static AudioClip CreateOrLoadVictory(string assetPath) => CreateOrLoad(assetPath, 0.60f, 660f, 0.35f);

        public static AudioClip CreateOrLoadPopupOpen(string assetPath) => CreateOrLoad(assetPath, 0.15f, 720f, 0.35f);

        public static AudioClip CreateOrLoadPopupClose(string assetPath) => CreateOrLoad(assetPath, 0.15f, 480f, 0.35f);

        private static AudioClip CreateOrLoad(string assetPath, float lengthSeconds, float frequency, float amplitude)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            var fullPath = Path.GetFullPath(assetPath);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            WriteSineWav(fullPath, lengthSeconds, frequency, amplitude);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        }

        private static void WriteSineWav(string fullPath, float lengthSeconds, float frequency, float amplitude)
        {
            var sampleCount = Mathf.Max(1, (int)(SampleRate * lengthSeconds));
            const short channels = 1;
            const short bitsPerSample = 16;
            var byteRate = SampleRate * channels * (bitsPerSample / 8);
            var dataSize = sampleCount * channels * (bitsPerSample / 8);

            using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            using var writer = new BinaryWriter(stream);

            writer.Write(new[] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + dataSize);
            writer.Write(new[] { 'W', 'A', 'V', 'E' });

            writer.Write(new[] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1);
            writer.Write(channels);
            writer.Write(SampleRate);
            writer.Write(byteRate);
            writer.Write((short)(channels * (bitsPerSample / 8)));
            writer.Write(bitsPerSample);

            writer.Write(new[] { 'd', 'a', 't', 'a' });
            writer.Write(dataSize);

            var clampedAmplitude = Mathf.Clamp01(amplitude);
            for (var i = 0; i < sampleCount; i++)
            {
                var t = (float)i / SampleRate;
                // Simple linear fade-out to avoid an end-of-clip click.
                var envelope = 1f - ((float)i / sampleCount);
                var sample = Mathf.Sin(2f * Mathf.PI * frequency * t) * clampedAmplitude * envelope;
                writer.Write((short)(sample * short.MaxValue));
            }
        }
    }
}
