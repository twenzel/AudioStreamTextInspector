using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AudioStreamTextInspector
{
    public class StreamErrorEventArgs: HandledEventArgs
    {
        public WebException Error { get; }

        public StreamErrorEventArgs(WebException ex) => Error = ex;
    }
}
