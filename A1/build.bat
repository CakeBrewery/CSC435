gplex CbLexer.l
gppg /gplex CbParser.y > CbParser.cs
csc /r:QUT.ShiftReduceParser.dll CbLexer.cs CbParser.cs cbc.cs