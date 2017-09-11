using AudioStreamTextInspector.Internal;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Speech.Recognition;
using System.Threading.Tasks;

namespace AudioStreamTextInspector
{
    public class Inspector : IDisposable
    {
        /// <summary>
        /// Gets the slicer instance
        /// </summary>
        public StreamSlicer Slicer { get; }

        /// <summary>
        /// Gets the recognizer instance
        /// </summary>
        public TextRecognizer Recognizer { get; }

        public event EventHandler<SpeechRecognizedEventArgs> OnTextRecognized;

        public Inspector(string region, string audioStreamUrl)
        {
            if (string.IsNullOrEmpty(region))
                throw new ArgumentException("No region given", nameof(region));

            if (string.IsNullOrEmpty(audioStreamUrl))
                throw new ArgumentNullException("Please provide an url to the stream", nameof(audioStreamUrl));

            Recognizer = new TextRecognizer(region);
            Recognizer.OnTextRecognized += (s, e) => OnTextRecognized?.Invoke(s, e);

            Slicer = new StreamSlicer(audioStreamUrl);
            Slicer.OnNewSlice += (s, m) => Recognizer.AddNewSlice(m);
        }

        /// <summary>
        /// Define the words which should be recognized
        /// </summary>
        /// <param name="words"></param>
        public void SetWordsToRecognise(params string[] words)
        {
            Recognizer.SetWordsToRecognise(words);
        }

        /// <summary>
        /// Starts the inspection
        /// </summary>
        public void Start()
        {
            Task.Run( ()=> Slicer.Start());
        }

        /// <summary>
        /// Stops the inspection
        /// </summary>
        public void Stop()
        {
            Slicer.Stop();
            Recognizer.Stop();
        }

        public void Dispose()
        {
            Stop();
            Recognizer?.Dispose();
        }
    }
}
