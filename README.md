# AudioStreamTextInspector

[![Build status](https://ci.appveyor.com/api/projects/status/pi0k1uyvkhq77ucy?svg=true)](https://ci.appveyor.com/project/twenzel/audiostreamtextinspector)
[![NuGet Version](http://img.shields.io/nuget/v/AudioStreamTextInspector.svg?style=flat)](https://www.nuget.org/packages/AudioStreamTextInspector/) 
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

Inspects mp3 audio streams and searches for texts/words

## Usage
```csharp
using (var inspector = new Inspector("de-DE", "http://stream.radio7.de/stream7/livestream.mp3?context=fHA6LTE="))
{
    inspector.SetWordsToRecognise("radio", "sieben", "verkehr", "rechnung", "wunsch");
    inspector.OnTextRecognized += Inspector_OnTextRecognized;
   
    inspector.Start();


    Console.WriteLine("Press any key to exit");
    Console.ReadLine();

    inspector.Stop();
}

private static void Inspector_OnTextRecognized(object sender, SpeechRecognizedEventArgs e)
{
	Console.WriteLine($"Recognized text:  {e.Result.Text}, {e.Result.Confidence} {e.Result.Audio.AudioPosition}");
}
```