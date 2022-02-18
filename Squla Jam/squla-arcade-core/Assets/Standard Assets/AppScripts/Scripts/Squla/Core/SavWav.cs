//	Copyright (c) 2012 Calvin Rien
//        http://the.darktable.com
//
//	This software is provided 'as-is', without any express or implied warranty. In
//	no event will the authors be held liable for any damages arising from the use
//	of this software.
//
//	Permission is granted to anyone to use this software for any purpose,
//	including commercial applications, and to alter it and redistribute it freely,
//	subject to the following restrictions:
//
//	1. The origin of this software must not be misrepresented; you must not claim
//	that you wrote the original software. If you use this software in a product,
//	an acknowledgment in the product documentation would be appreciated but is not
//	required.
//
//	2. Altered source versions must be plainly marked as such, and must not be
//	misrepresented as being the original software.
//
//	3. This notice may not be removed or altered from any source distribution.
//
//  =============================================================================
//
//  derived from Gregorio Zanon's script
//  http://forum.unity3d.com/threads/119295-Writing-AudioListener.GetOutputData-to-wav-problem?p=806734&viewfull=1#post806734

using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class SavWav {

	const int HEADER_SIZE = 44;

	public static Byte[] ConvertToByteArray(AudioClip clip)
	{
		Byte[] data = new Byte[clip.samples * 2];
		Byte[] header = new Byte[HEADER_SIZE];

		int dataLength;

		data = ConvertAndWrite(clip);

		dataLength = data.Length;
		header = WriteHeader(clip, dataLength);

		Byte[] buffer = new Byte[header.Length + dataLength];
		header.CopyTo(buffer, 0);
		data.CopyTo(buffer, header.Length);

		return buffer;
	}

	static Byte[] ConvertAndWrite(AudioClip clip) {

		var samples = new float[clip.samples];

		clip.GetData(samples, 0);

		Int16[] intData = new Int16[samples.Length];
		Byte[] bytesData = new Byte[samples.Length * 2];
		const float rescaleFactor = 32767;

		for (int i = 0; i < samples.Length; i++)
		{
			intData[i] = (short)(samples[i] * rescaleFactor);
		}

		Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);
		return bytesData;
	}

	static Byte[] WriteHeader(AudioClip clip, int dataLength)
	{
		List<Byte> buffer = new List<Byte>();

		var hz = clip.frequency;
		var channels = clip.channels;
		var samples = clip.samples;

		Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");

		buffer.AddRange(riff.ToList());

		Byte[] chunkSize = BitConverter.GetBytes(dataLength + HEADER_SIZE - 8);
		buffer.AddRange(chunkSize.ToList());

		Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
		buffer.AddRange(wave.ToList());

		Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
		buffer.AddRange(fmt.ToList());

		Byte[] subChunk1 = BitConverter.GetBytes(16);
		buffer.AddRange(subChunk1.ToList());

		UInt16 one = 1;

		Byte[] audioFormat = BitConverter.GetBytes(one);
		buffer.AddRange(audioFormat.ToList());

		Byte[] numChannels = BitConverter.GetBytes(one);
		buffer.AddRange(numChannels.ToList());

		Byte[] sampleRate = BitConverter.GetBytes(hz);
		buffer.AddRange(sampleRate.ToList());

		Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
		buffer.AddRange(byteRate.ToList());

		UInt16 blockAlign = (ushort) (channels * 2);
		buffer.AddRange(BitConverter.GetBytes(blockAlign));

		UInt16 bps = 16;
		Byte[] bitsPerSample = BitConverter.GetBytes(bps);
		buffer.AddRange(bitsPerSample.ToList());

		Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
		buffer.AddRange(datastring.ToList());

		Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
		buffer.AddRange(subChunk2.ToList());

		return buffer.ToArray();
	}
}