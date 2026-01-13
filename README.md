CodeSentry - Static Analysis Tool
──────────────────────────────────────────────────
Found 1 .cs files

active analyzers:
   • TODO CHECKER
   • Long line checker
   • God class checker

Results:
──────────────────────────────────────────────────

 ./UserManager.cs
   Analyzer: TODO CHECKER
   • line 8: found TODO comment
   • line 24: found TODO comment
   • line 96: found TODO comment
   • line 188: found TODO comment
   • line 283: found TODO comment

 ./UserManager.cs
   Analyzer: Long line checker
   • Line 25: too large line (195 chars, max: 120)
   • Line 29: too large line (139 chars, max: 120)
   • Line 30: too large line (177 chars, max: 120)
   • Line 277: too large line (157 chars, max: 120)

 ./UserManager.cs
   Analyzer: God class checker
   • God class detected! The file has 505 lines (max: 500)

──────────────────────────────────────────────────
 10 issues in 3 files
