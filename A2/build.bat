call rungplex
call rungppg
csc /debug:full /r:QUT.ShiftReduceParser.dll CbLexer.cs CbParser.cs CbAST.cs CbType.cs CbTopLevel.cs CbVisitor.cs CbPrVisitor.cs NsVisitor.cs cbc.cs
