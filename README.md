# UNFINISHED!!!

# DirectUI UI File Compiler

A better compiler for UI files for Microsoft's private DirectUI framework.

This has improved support for UI file includes (like the C preprocessor) and supports CSS-like stylesheet syntax on post-Windows-XP versions of DirectUI.

## Features

### Preprocessor

The preprocessor is a reduced dialect of the C preprocessor. It only supports `#include`, argument-less `#define` macros, and basic branching (`#if`, `#ifndef`, `#else`, etc.)

This is done because Microsoft did the same thing, but with the actual C preprocessor. This reduced implementation avoids header contents meant for C or C++ from spilling into the DirectUI UI file, in the case where you're targeting an XML output and not a compressed DUIB file.

### Stylesheets

Stylesheets are defined in a CSS-like language.