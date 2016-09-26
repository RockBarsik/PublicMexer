﻿using System;
using System.Threading;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System.Threading.Tasks;

namespace SoundGenerator
{
    public class SineGenerator
    {
        SourceVoice _sourceVoice;
        XAudio2 _xaudio2;
        MasteringVoice _masteringVoice;
        WaveFormat _waveFormat;
        DataStream _dataStream;
        int _bufferSize;
        public bool stopFlag { get; set; }

        int _valueRate;
        float _valueAmp;

        public SineGenerator()
        {
            _xaudio2 = new XAudio2();
            _masteringVoice = new MasteringVoice(_xaudio2);

            _waveFormat = new WaveFormat(44100, 32, 2);
            _sourceVoice = new SourceVoice(_xaudio2, _waveFormat);

            _bufferSize = _waveFormat.ConvertLatencyToByteSize (60000);
            _dataStream = new DataStream(_bufferSize, true, true);

            _valueRate = 0;
            _valueAmp = 0.5f;
            stopFlag = true;
        }

        public void AddValue(int value = 10)
        {
           _valueRate += value;
        }

        public void AddValueAmp(float value)
        {
            _valueAmp += value;
        }

        public async Task Generate()
        {
            while (!stopFlag)
            {
                int numberOfSamples = _bufferSize / _waveFormat.BlockAlign;
                for (int i = 0; i < numberOfSamples; i++)
                {
                    float value = (float)(Math.Sin(2 * Math.PI * _valueRate * i / _waveFormat.SampleRate) * _valueAmp);
                    _dataStream.Write(value);
                }
                _dataStream.Position = 0;

                var audioBuffer = new AudioBuffer { Stream = _dataStream, Flags = BufferFlags.EndOfStream, AudioBytes = _bufferSize };

                _sourceVoice.Stop();
                _sourceVoice.SubmitSourceBuffer(audioBuffer, null);
                _sourceVoice.Start();
                Console.Out.Flush();
                await Task.Delay(1100);
            }
            _valueRate = 0;
        }
    }
}
