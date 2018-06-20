# Wakek

Summary: this repository contains an application in which you can select a http request benchmark and execute it

## What does *Wakek* mean?

Wakek is the vulcan word for timer

## Can it be useful to you?

Some of the functionality is very specific for my php website, but basically you can benchmark your own website by adding a benchmark definition to the BenchmarkDefinitions secret, e.g.

```
  <BenchmarkDefinition>
    <description>My Own Website</description>
    <url>https://www.myownwebsite.de/</url>
    <executiontype>CsNative</executiontype>
    <timeinseconds>5</timeinseconds>
    <callsinparallel>1</callsinparallel>
  </BenchmarkDefinition>
```
