# Introduction

You have been provided with a console application project containing a `Program` class and an 
`ExampleData` class. 

The `ExampleData` class contains example CSV data containing organisation contact information in no
 particular order.

* The first column is the Row ID, a unique sequence number assigned to that row.
* The second column links the row to its parent record within the file, which may in turn link to 
another record, etc.

The task is to update the application to display an "Organogram" (a hierarchical view of an 
organisation).

# Objective

* Run through the records and print an "Organogram" in a hierarchical view.
* Within each level of the hierarchy, records should be printed in ascending order by Row ID.
* Code should be able to cater for n-level depth.
* Code should be written in a testable way. If you are familiar with unit tests and/or acceptance 
tests, please include them.

# Example

```
Peter Ndoro, IBM, Managing Director
 -> Jackie Smith, IBM, Assistant Director
 -> Chris Thorpe, IBM, Technical Director
    -> John Major, IBM, Lead Developer
       -> Peter South, IBM, Senior Developer
       -> James McDonald, IBM, Developer
```

# How to submit

When you are ready to submit, run `dotnet run submit.cs` from the root of your solution. This
creates a single file, `submission.patch`, containing your complete solution and commit history.
Email this file back to us - no other files are required.

> Tip: commit your work incrementally as you go. The commit history is part of what we review.

© Copyright 2026 Acturis Ltd.