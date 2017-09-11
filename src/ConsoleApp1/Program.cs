using AudioStreamTextInspector;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
       

        //private static volatile bool fullyDownloaded;
        //private static SpeechRecognitionEngine _recognizer;
        //private static MemoryStream _ms;
        //private static StreamingPlaybackState playbackState;
        ////private static System.Timers.Timer _timer;
        //private static WaveFileWriter _writer;

        static void Main(string[] args)
        {
            //_timer = new System.Timers.Timer(250);
            //_timer.Elapsed += _timer_Elapsed;
            //testFile();
            //getFromStream();

            using (var inspector = new Inspector("de-DE", "http://stream.radio7.de/stream7/livestream.mp3?context=fHA6LTE="))
            {
                inspector.SetWordsToRecognise("radio", "sieben", "verkehr", "laser", "rechnung", "wunsch");
                inspector.OnTextRecognized += Inspector_OnTextRecognized;

                inspector.Recognizer.Engine.AudioSignalProblemOccurred += Recognizer_AudioSignalProblemOccurred;
                inspector.Recognizer.Engine.AudioStateChanged += Recognizer_AudioStateChanged;
                inspector.Recognizer.Engine.RecognizerUpdateReached += Recognizer_RecognizerUpdateReached;
                inspector.Recognizer.Engine.SpeechDetected += Recognizer_SpeechDetected;
                inspector.Recognizer.Engine.SpeechHypothesized += Recognizer_SpeechHypothesized;
                inspector.Recognizer.Engine.SpeechRecognitionRejected += Recognizer_SpeechRecognitionRejected;

                inspector.Start();


                Console.WriteLine("Press any key to exit");
                Console.ReadLine();

                inspector.Stop();
            }
        }

        private static void Inspector_OnTextRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine($"!!! Recognized text:  {e.Result.Text}, {e.Result.Confidence} {e.Result.Audio.AudioPosition}");
        }

        //private static void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    if (playbackState != StreamingPlaybackState.Stopped)
        //    {
        //        if (bufferedWaveProvider != null)
        //        {
        //            var bufferedSeconds = bufferedWaveProvider.BufferedDuration.TotalSeconds;
        //            ShowBufferState(bufferedSeconds);
        //            // make it stutter less if we buffer up a decent amount before playing
        //            //if (bufferedSeconds < 0.5 && playbackState == StreamingPlaybackState.Playing && !fullyDownloaded)
        //            //{
        //            //    Pause();
        //            //}
        //            //else if (bufferedSeconds > 4 && playbackState == StreamingPlaybackState.Buffering)
        //            //{
        //            //    Play();
        //            //}
        //            //else if (fullyDownloaded && bufferedSeconds == 0)
        //            //{
        //            //    Debug.WriteLine("Reached end of stream");
        //            //    StopPlayback();
        //            //}
        //        }

        //    }
        //}

        //private static void Play()
        //{            

        //    var info = new System.Speech.AudioFormat.SpeechAudioFormatInfo(44100, AudioBitsPerSample.Sixteen, AudioChannel.Stereo);

        //    if (_recognizer == null)
        //    {
        //        _recognizer = CreateRecognizer();
        //    }

        //    // Start asynchronous, continuous speech recognition.  

        //    _recognizer.SetInputToAudioStream(_ms, info);
        //    _recognizer.RecognizeAsync();
        //    playbackState = StreamingPlaybackState.Playing;
        //}

        //private static void CopyBufferToReaderStream()
        //{
        //    byte[] buffer = new byte[16384 * 4];
        //    int read;
        //    _ms = new MemoryStream();
        //    while (bufferedWaveProvider.BufferedBytes > 0)
        //    {
        //        var len = Math.Min(bufferedWaveProvider.BufferedBytes, buffer.Length);
        //        if ((read = bufferedWaveProvider.Read(buffer, 0, len)) > 0)
        //            _ms.Write(buffer, 0, read);
        //    }
        //}

        //private static void Pause()
        //{
        //    playbackState = StreamingPlaybackState.Buffering;       
        //}

        //private static void StopPlayback()
        //{
        //    if (playbackState != StreamingPlaybackState.Stopped)
        //    {
        //        if (!fullyDownloaded)
        //        {
        //            //webRequest.Abort();
        //        }

        //        playbackState = StreamingPlaybackState.Stopped;

        //        //_timer.Enabled = false;
        //        // n.b. streaming thread may not yet have exited
        //        Thread.Sleep(500);
        //        ShowBufferState(0);
        //    }
        //}

        //private static void ShowBufferState(double totalSeconds)
        //{
        //    Console.WriteLine(String.Format("Buffer state {0:0.0}s", totalSeconds));
        //}

        //private static void testFile()
        //{

        //    // Create an in-process speech recognizer for the en-US locale.


        //    //using (Mp3FileReader reader = new Mp3FileReader("test.mp3"))
        //    //{
        //    //    WaveFileWriter.CreateWaveFile("convert.wav", reader);
        //    //}
        //    fullyDownloaded = false;
        //    var buffer = new byte[16384 * 4]; // needs to be big enough to hold a decompressed frame            
        //    //_timer.Start();

        //    IMp3FrameDecompressor decompressor = null;
        //    try
        //    {


        //        using (var responseStream = WebRequest.Create("http://stream.radio7.de/stream7/livestream.mp3?context=fHA6LTE=").GetResponse().GetResponseStream())
        //        {
        //            var readFullyStream = new ReadFullyStream(responseStream);
        //            do
        //            {
        //                if (IsBufferNearlyFull)
        //                {
        //                    Debug.WriteLine("Buffer getting full, taking a break");
        //                    //CopyBufferToReaderStream();
        //                    //Play();
        //                }
        //                else
        //                {
        //                    Mp3Frame frame;
        //                    try
        //                    {
        //                        frame = Mp3Frame.LoadFromStream(readFullyStream);
        //                    }
        //                    catch (EndOfStreamException)
        //                    {
        //                        fullyDownloaded = true;
        //                        // reached the end of the MP3 file / stream
        //                        break;
        //                    }
        //                    catch (WebException)
        //                    {
        //                        // probably we have aborted download from the GUI thread
        //                        break;
        //                    }
        //                    if (decompressor == null)
        //                    {
        //                        // don't think these details matter too much - just help ACM select the right codec
        //                        // however, the buffered provider doesn't know what sample rate it is working at
        //                        // until we have a frame
        //                        decompressor = CreateFrameDecompressor(frame);                                
        //                    }                           

        //                    int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
        //                    //Debug.WriteLine(String.Format("Decompressed a frame {0}", decompressed));
        //                    //bufferedWaveProvider.AddSamples(buffer, 0, decompressed);

        //                    if (_ms == null)
        //                    {
        //                        _ms = new MemoryStream();
        //                        _writer = new WaveFileWriter(_ms, decompressor.OutputFormat);

        //                    }
        //                    _writer.Write(buffer, 0, decompressed);

        //                    if (_ms.Length > decompressor.OutputFormat.AverageBytesPerSecond * 20) // 20 sec
        //                    {                                                                
        //                        _writer.Flush();
        //                        _writer.Close();                                
        //                        _ms = null;
        //                    }

        //                }

        //            } while (true);
        //            Debug.WriteLine("Exiting");
        //            // was doing this in a finally block, but for some reason
        //            // we are hanging on response stream .Dispose so never get there
        //            decompressor.Dispose();
        //        }

        //    }
        //    finally
        //    {
        //        if (decompressor != null)
        //        {
        //            decompressor.Dispose();
        //        }
        //    }


        //    //using (Stream stream = WebRequest.Create("http://stream.radio7.de/stream7/livestream.mp3?context=fHA6LTE=").GetResponse().GetResponseStream())
        //    //{
        //    //    //var t = new StreamMediaFoundationReader(stream);
        //    //    using (MemoryStream ms = new MemoryStream())
        //    //    {
        //    //        byte[] buffer = new byte[16384 * 4];
        //    //        int read;
        //    //        if ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        //    //        {
        //    //            ms.Write(buffer, 0, read);

        //    //            for (int i = 0; i < 2; i++)
        //    //            {
        //    //                if ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        //    //                    ms.Write(buffer, 0, read);
        //    //            }

        //    //            using (Mp3FileReader mp3 = new Mp3FileReader(ms))
        //    //            {
        //    //                using (WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(mp3))
        //    //                {
        //    //                    //        WaveFileWriter.CreateWaveFile(save1.FileName, pcm);
        //    //                    //    }
        //    //                    //}




        //    //                    var info = new System.Speech.AudioFormat.SpeechAudioFormatInfo(44100, AudioBitsPerSample.Sixteen, AudioChannel.Stereo);


        //    //                    recognizer.SetInputToAudioStream(pcm, info);

        //    //                    // Start asynchronous, continuous speech recognition.
        //    //                    recognizer.RecognizeAsync();


        //    //                    Console.WriteLine("done");

        //    //                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        //    //                    {
        //    //                        ms.Write(buffer, 0, read);

        //    //                    }
        //    //                    Console.ReadLine();

        //    //                }
        //    //            }
        //    //        }
        //    //    }
        //    //}

        //}

        //private static bool IsBufferNearlyFull
        //{
        //    get
        //    {
        //        return bufferedWaveProvider != null &&
        //               bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes
        //               < bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4;
        //    }
        //}

        //private static SpeechRecognitionEngine CreateRecognizer()
        //{
        //    var recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("de-DE"));

        //    var builder = new GrammarBuilder("radio zahlt's wenzel wetter toni verkehr Laser ");
        //    recognizer.LoadGrammar(new Grammar(builder) { Name = "fff" });

        //    var greeting = new Choices(new string[] { "radio", "verkehr", "laser", "uhr", "rechnung", "wunsch" });
        //    recognizer.LoadGrammar(new Grammar(new GrammarBuilder(greeting)) { Name = "zzz" });

        //    // Add a handler for the speech recognized event.
        //    recognizer.SpeechRecognized +=
        //      new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);
        //    recognizer.RecognizeCompleted += Recognizer_RecognizeCompleted;

        //    // Attach an event handler for the LoadGrammarCompleted event.
        //    recognizer.LoadGrammarCompleted += new EventHandler<LoadGrammarCompletedEventArgs>(recognizer_LoadGrammarCompleted);
        //    recognizer.AudioSignalProblemOccurred += Recognizer_AudioSignalProblemOccurred;
        //    recognizer.AudioStateChanged += Recognizer_AudioStateChanged;
        //    recognizer.RecognizerUpdateReached += Recognizer_RecognizerUpdateReached;
        //    recognizer.SpeechDetected += Recognizer_SpeechDetected;
        //    recognizer.SpeechHypothesized += Recognizer_SpeechHypothesized;
        //    recognizer.SpeechRecognitionRejected += Recognizer_SpeechRecognitionRejected;

        //    return recognizer;
        //}

        //private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame)
        //{
        //    WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
        //        frame.FrameLength, frame.BitRate);
        //    return new AcmMp3FrameDecompressor(waveFormat);
        //}

        private static void Recognizer_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Console.WriteLine($"SpeechRecognitionRejected: " + e.Result);
        }

        private static void Recognizer_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            Console.WriteLine($"!!! SpeechHypothesized: {e.Result.Text}, {e.Result.Confidence}");
        }

        private static void Recognizer_SpeechDetected(object sender, SpeechDetectedEventArgs e)
        {
            Console.WriteLine($"SpeechDetected: " + e.AudioPosition);
        }

        private static void Recognizer_RecognizerUpdateReached(object sender, RecognizerUpdateReachedEventArgs e)
        {
            Console.WriteLine($"RecognizerUpdateReached: " + e.AudioPosition);
        }

        private static void Recognizer_AudioStateChanged(object sender, AudioStateChangedEventArgs e)
        {
            //Console.WriteLine($"AudioStateChanged: " + e.AudioState);
        }

        private static void Recognizer_AudioSignalProblemOccurred(object sender, AudioSignalProblemOccurredEventArgs e)
        {
            Console.WriteLine($"AudioSignalProblemOccurred: {e.AudioSignalProblem} @ {e.AudioPosition}");
        }

        private static void Recognizer_RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            Console.WriteLine($"RecognizeCompleted: {e.Result}");
        }

        //private static void getFromStream()
        //{
        //    // Create an in-process speech recognizer for the en-US locale.
        //    using (SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("de-DE")))
        //    {

        //        //// Create a default dictation grammar.
        //        //DictationGrammar defaultDictationGrammar = new DictationGrammar();
        //        //defaultDictationGrammar.Name = "default dictation";
        //        //defaultDictationGrammar.Enabled = true;

        //        //// Create the spelling dictation grammar.
        //        //DictationGrammar spellingDictationGrammar =
        //        //  new DictationGrammar("grammar:dictation#spelling");
        //        //spellingDictationGrammar.Name = "spelling dictation";
        //        //spellingDictationGrammar.Enabled = true;

        //        //// Create the question dictation grammar.
        //        //DictationGrammar customDictationGrammar =
        //        //  new DictationGrammar("grammar:dictation");
        //        //customDictationGrammar.Name = "question dictation";
        //        //customDictationGrammar.Enabled = true;

        //        //recognizer.LoadGrammar(defaultDictationGrammar);
        //        //recognizer.LoadGrammar(spellingDictationGrammar);
        //        //recognizer.LoadGrammar(customDictationGrammar);

        //        //// Add a context to customDictationGrammar.
        //        //customDictationGrammar.SetDictationContext("radio 7", null);

        //        var builder = new GrammarBuilder("radio 7 zahlt's wenzel wetter toni");
        //        recognizer.LoadGrammarAsync(new Grammar(builder));

        //        // Add a handler for the speech recognized event.
        //        recognizer.SpeechRecognized +=
        //          new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);

        //        using (Stream stream = WebRequest.Create("http://stream.radio7.de/stream7/livestream.mp3?context=fHA6LTE=").GetResponse().GetResponseStream())
        //        {

        //            //// Create the buffer
        //            //int numberOfPcmBytesToReadPerChunk = 512;
        //            //byte[] buffer = new byte[numberOfPcmBytesToReadPerChunk];

        //            //int bytesReturned = -1;
        //            //int totalBytes = 0;
        //            //while (bytesReturned != 0)
        //            //{
        //            //    bytesReturned = stream.Read(buffer, 0, buffer.Length);
        //            //    totalBytes += bytesReturned;
        //            //}




        //            using (MemoryStream ms = new MemoryStream())
        //            {
        //                //using (Mp3Stream mp3Stream = new Mp3Stream(ms))
        //                //{
        //                //    byte[] buffer = new byte[32768];
        //                //    int read;
        //                //    if ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        //                //    {
        //                //        ms.Write(buffer, 0, read);

        //                //        File.WriteAllBytes("testc.mp3", ms.ToArray());

        //                //        var info = new System.Speech.AudioFormat.SpeechAudioFormatInfo(44100, AudioBitsPerSample.Sixteen, AudioChannel.Stereo);

        //                //        // Configure input to the speech recognizer.
        //                //        recognizer.SetInputToAudioStream(ms, info);

        //                //        // Start asynchronous, continuous speech recognition.
        //                //        recognizer.RecognizeAsync(RecognizeMode.Multiple);

        //                //        // Keep the console window open.
        //                //        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        //                //        {
        //                //            ms.Write(buffer, 0, read);
        //                //            File.WriteAllBytes("testc.mp3", ms.ToArray());
        //                //        }
        //                //    }

        //                //    Console.WriteLine("done");
        //                //    Console.ReadLine();
        //                //}
        //            }
        //        }
        //    }
        //}



        //static void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        //{
        //    Console.WriteLine($"!!! Recognized text:  {e.Result.Text}, {e.Result.Confidence} {e.Result.Audio.AudioPosition}");
        //}

        //static void recognizer_LoadGrammarCompleted(object sender, LoadGrammarCompletedEventArgs e)
        //{
        //    Console.WriteLine("Grammar loaded: " + e.Grammar.Name);
        //}
    }
}
