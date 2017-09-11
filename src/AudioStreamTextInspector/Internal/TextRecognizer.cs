using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using System.Text;

namespace AudioStreamTextInspector.Internal
{
    public class TextRecognizer : IDisposable
    {
        private ConcurrentQueue<MemoryStream> _slices = new ConcurrentQueue<MemoryStream>();
        private Grammar _wordsGrammar;
        private bool _isWorking;

        public SpeechRecognitionEngine Engine { get; }
        public event EventHandler<SpeechRecognizedEventArgs> OnTextRecognized;

        public TextRecognizer(string region)
        {
            if (string.IsNullOrEmpty(region))
                throw new ArgumentException("No region given", nameof(region));

            Engine = new SpeechRecognitionEngine(new System.Globalization.CultureInfo(region));
            Engine.SpeechRecognized += Engine_SpeechRecognized;
            Engine.RecognizeCompleted += Engine_RecognizeCompleted;
        }

        public void Dispose()
        {
            Engine?.Dispose();
        }

        /// <summary>
        /// Adds a new slice to the queue;
        /// </summary>
        /// <param name="slice"></param>
        public void AddNewSlice(MemoryStream slice)
        {
            _slices.Enqueue(slice);
            StartNextSlice();
        }

        /// <summary>
        /// Define the words which should be recognized
        /// </summary>
        /// <param name="words"></param>
        public void SetWordsToRecognise(params string[] words)
        {
            if (_wordsGrammar != null)
                Engine.UnloadGrammar(_wordsGrammar);

            var choices = new Choices(words);
            _wordsGrammar = new Grammar(new GrammarBuilder(choices));

            Engine.LoadGrammar(_wordsGrammar);
        }

        /// <summary>
        /// Stops the engine
        /// </summary>
        public void Stop()
        {
            Engine.RecognizeAsyncStop();
        }

        private void StartNextSlice()
        {
            if (!_isWorking && _slices.TryDequeue(out var slice))
            {
                _isWorking = true;
                var info = new System.Speech.AudioFormat.SpeechAudioFormatInfo(44100, AudioBitsPerSample.Sixteen, AudioChannel.Stereo);

                Engine.SetInputToAudioStream(slice, info);
                Engine.RecognizeAsync();
            }
        }

        private void Engine_RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            _isWorking = false;
            StartNextSlice();
        }

        private void Engine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            OnTextRecognized?.Invoke(sender, e);
        }
    }
}
