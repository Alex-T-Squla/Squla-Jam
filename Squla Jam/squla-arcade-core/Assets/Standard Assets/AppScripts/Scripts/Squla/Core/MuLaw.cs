using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public static class MuLaw
{
	public const string ENCODING_CODE = "MULAW";
	public const int DEFAULT_HZ = 16000;
	public const int DEFAULT_CHANNELS = 1;
	const int CLIP = 32635;
	const int BIAS = 0x84;
	static readonly int[] ENCODING_TABLE = {
		0, 0, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3,
		4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
		6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
		6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
		6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7
	};

	const int HEADER_SIZE = 58;

	public static float[] InterpolateArray(float[] samples, double newSampleRate, double oldSampleRate)
	{
		const float rescaleFactor = 32767; 
		int fitCount = (int) Math.Round((double) (samples.Length * (newSampleRate / oldSampleRate)));
		float[] newData = new float[fitCount];
		float springFactor = ((float) samples.Length - 1) / ((float) fitCount - 1);
		newData[0] = samples[0] * rescaleFactor; // for new allocation
		for (uint i = 1; i < fitCount - 1; i++)
		{
			float tmp = i * springFactor;
			int before = (int) Math.Floor(tmp);
			int after =  (int) Math.Ceiling(tmp);
			float atPoint = tmp - before;
			newData[i] = LinearInterpolate(samples[before] * rescaleFactor, samples[after] * rescaleFactor, atPoint);
		}

		newData[fitCount - 1] = samples[samples.Length - 1] * rescaleFactor; // for new allocation
		return newData;
	}

	public static float LinearInterpolate(float before, float after, float atPoint)
	{
		return before + (after - before) * atPoint;
	}

	public static byte EncodeSample(short originalSample)
	{
		// Extend the sign bit for the sample that is constructed from two bytes
		int sample = ((originalSample >> 2) << 2);

		/** get the sample into sign-magnitude **/
		int sign = (sample >> 8) & 0x80;
		if (sign != 0)
		{
			sample = -sample;
		}

		if (sample > CLIP)
		{
			sample = CLIP;
		}

		/** convert from 16 bit linear to ulaw **/
		sample += BIAS;
		int exponent = ENCODING_TABLE[(sample >> 7) & 0xFF];
		int mantissa = (sample >> (exponent + 3)) & 0x0F;
		byte muLawSample = (byte) ~(sign | (exponent << 4) | mantissa);

		return muLawSample;
	}

	public static byte[] ConvertToByteArray(AudioClip clip)
	{
		byte[] header = new byte[HEADER_SIZE];
		int dataLength;

		// Interpolate down to 16KHz
		float[] samples = new float[clip.samples];
		clip.GetData(samples, 0);
		samples = InterpolateArray(samples, 16000, clip.frequency);

		// Convert to MuLaw
		byte[] data = new byte[samples.Length + 1];
		data = ConvertAndWrite(samples);

		dataLength = data.Length;
		header = WriteHeader(clip, dataLength);

		byte[] buffer = new byte[header.Length + dataLength];
		header.CopyTo(buffer, 0);
		data.CopyTo(buffer, header.Length);

		/* Use this to test your recording */
		// string path = "Assets/Resources/test.wav";
		// File.WriteAllBytes(path, buffer);
			
		return buffer;
	}

	static byte[] ConvertAndWrite(float[] samples)
	{
		byte[] bytesData = new byte[samples.Length];

		for (int i = 0; i < samples.Length; i++)
		{
			bytesData[i] = EncodeSample((short)(samples[i]));
		}

		return bytesData;
	}

	static byte[] WriteHeader(AudioClip clip, int dataLength)
	{
		List<byte> buffer = new List<byte>();

		int samples = clip.samples;
		int hz = DEFAULT_HZ;
		int channels = DEFAULT_CHANNELS;

		byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");

		buffer.AddRange(riff.ToList());

		byte[] chunkSize = BitConverter.GetBytes(dataLength + HEADER_SIZE - 8);
		buffer.AddRange(chunkSize.ToList());

		byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
		buffer.AddRange(wave.ToList());

		byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
		buffer.AddRange(fmt.ToList());

		byte[] subChunk1 = BitConverter.GetBytes(18);
		buffer.AddRange(subChunk1.ToList());

		ushort format = 7;
		ushort one = 1;

		byte[] audioFormat = BitConverter.GetBytes(format);
		buffer.AddRange(audioFormat.ToList());

		byte[] numChannels = BitConverter.GetBytes(one);
		buffer.AddRange(numChannels.ToList());

		byte[] sampleRate = BitConverter.GetBytes(hz);
		buffer.AddRange(sampleRate.ToList());

		byte[] byteRate = BitConverter.GetBytes(hz);
		buffer.AddRange(byteRate.ToList());

		ushort blockAlign = (ushort)(channels);
		buffer.AddRange(BitConverter.GetBytes(blockAlign));

		ushort bps = 8;
		buffer.AddRange(BitConverter.GetBytes(bps));

		ushort zero = 0;
		buffer.AddRange(BitConverter.GetBytes(zero));

		byte[] factstring = System.Text.Encoding.UTF8.GetBytes("fact");
		buffer.AddRange(factstring.ToList());

		ushort four = 4;
		byte[] padding = BitConverter.GetBytes(four);
		buffer.AddRange(padding.ToList());

		// 00 00
		buffer.AddRange(BitConverter.GetBytes(zero));

		buffer.AddRange(chunkSize.ToList());

		byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
		buffer.AddRange(datastring.ToList());

		byte[] subChunk2 = chunkSize;
		buffer.AddRange(subChunk2.ToList());

		return buffer.ToArray();
	}
}