# Contributing

Here you find some guidelines for your contributions (this document is inspired by 
[these guidelines](https://github.com/thestonefox/SteamVR_Unity_Toolkit/blob/master/CONTRIBUTING.md)).

## Getting Started
Make sure you have a GitHub account

### Opening an issue
Before creating a new issue please look for similar issues already exists:
  * in case you found it
    * read it carefully
    * add your comments to the existing issue
	* add comment to an issue only if related to it (otherwise choose a different issue or create a new one).
  * otherwise create a new issue on the GitHub repository
     * Clearly describe the issue
	 * do not write long text in issue title, choose a short title for the topic, then add your text to the issue body
     * if it's a bug include also steps to reproduce it
     * Make sure you fill in the earliest version that you know has
   the issue

If you just want to notify bugs or suggesting improvements you can just open an issue on GitHub, otherwise
[Fork] the repository on GitHub, then make your changes and create a pull request (you can find a brief tutorial for [SourceTree] users at the end of this document).

### Making Changes
Follow these steps, paying attention to adhere to the rules described in the next paragraphs.
* [Fork](http://help.github.com/fork-a-repo/) the project, clone your fork, and configure the remotes.
* Create a topic branch from where you want to base your work
  * This is usually the master branch
  * Only target release branches if you are certain your fix must be
   on that branch
  * Name branches with the type of issue you are fixing;
   `fix`, `feat`, `chore`, `docs` (e.g. `fix/my_bug_fix`)
  * Please avoid working directly on the master branch
* Make commits of logically related units (e.g. do not mix unrelated changes)
* Make sure your commit messages are in the proper format
* Push your topic branch up to your fork.
* [Open a Pull Request] with a clear title and description.

## Coding Conventions
We tried to follow the coding style described hereafter,
anyway if you find some not compliant code, you are encouraged to change
it (this should be classified as a `refactor` change).

To ensure all code is consistent and readable, we adhere to the
default coding conventions utilized in Visual Studio. The easiest
first step is to auto format the code within Visual Studio with
the key combination of `Ctrl + k` then `Ctrl + d` which will ensure
the code is correctly formatted and indented.

Spaces should be used instead of tabs for better readability across
a number of devices (e.g. spaces look better on GitHub source view.).
Always use a indention size of 4.

In regards to naming conventions we also adhere to the standard
.NET Framework naming convention system which can be
[viewed online here](https://msdn.microsoft.com/en-gb/library/x2dbyw72(v=vs.71).aspx).

Class methods and parameters should always denote their accessibility
level (`public`, `protected`, `private`).
* **Incorrect:** `void MyMethod()`
* **Correct:** **`private`**` void MyMethod()`

All core classes should be within the `AsImpL` namespace.
All example classes should be within the `AsImpL.Examples` namespace.


Always place curly braces (`{ }`) on a new line. 
Always use curly braces in conditional statements, even in case of a single statement. 

* **Incorrect:**

  ~~~
    if (this == that) DoThat();
    if (this == that) { DoThat(); }
  ~~~

* **Correct:**

  ~~~
    if (this == that)
    {
      DoThat();
    }
  ~~~

Declare each variable independently, not in the same statement.

For each event defined in a class, the method that triggers it must at least be `protected virtual` to allow any further inherited class to override and extend it.

It is acceptable to have multiple classes defined in the same file as
long as the subsequent defined classes are only used by the main class
defined in the same file (you should define them as `internal`).

Use public nested classes as long as they are strictly related to the class to which they belong.

Use the built-in data type aliases (`string`, `int`, ...), not the .NET
common type system (`String`, `Int32`, ...).

### Comments
Use `//` or `///` (for documentation), but never `/* ... */` in comments.
Use comments to explain the implementation or to motivate some choices,
do not use comments to explain obvious code.
Place a proper tag (`TODO:`, `NOTE:`, `HACK:`, `UNDONE:`) before the comments inside a method implementation for
annotating and documenting tasks.
These annotations are collected by Visual Studio to show a 
[task list](https://msdn.microsoft.com/en-us/library/txtwdysk(v=vs.140).aspx).

### Ordering
The ordering of fields, methods, etc. should follow the [StyleCop Rules], see the answer of [Jonathan Wright] in [StackOverflow].
Within a class, struct or interface: (SA1201 and SA1203)
* Constant Fields
* Fields
* Constructors
* Finalizers (Destructors)
* Delegates
* Events
* Enums
* Interfaces
* Properties
* Indexers
* Methods
* Structs
* Classes

Within each of these groups order by access: (SA1202)
* public
* internal
* protected internal
* protected
* private

Within each of the access groups, order by static, then non-static: (SA1204)
* static
* non-static

### Namespaces
Place namespace `using` statements together at the top of file,
in this order:
1. .NET namespaces
2. Unity namespaces
3. other namespaces

### Regions
Avoid using regions unless strictly necessary, in any case:
* do not use regions to group together fields, properties, methods, etc.;
  use the above mentioned ordering convention so they are already logically grouped;
* do not use regions inside methods, refactor instead.

See this [answer by Arseni Mourzenko on StackExchange](http://softwareengineering.stackexchange.com/questions/53086/are-regions-an-antipattern-or-code-smell).


## Documentation

All scripts that require documentation need to contain
inline code documentation adhering to the .NET Framework XML
documentation comments convention which can be
[viewed online here](https://msdn.microsoft.com/en-us/library/b2s063f7.aspx)

Public classes, methods, delegate events and unity events should be
documented using the XML comments and contain a line `<summary>`
with any additional lines included in `<remarks>`.

Public fields that appear in the inspector require a `[Tooltip("")]` to get a proper tooltip in Unity Editor.

The [AsImpL documentation] is automatically generated using [Doxygen].

## Commit Messages

The commit message lines should never exceed 72 characters and should
be entered in the following format:

~~~
type(scope): subject
(blank line)
body
(new line at the end)
~~~
See below for details.

### Type

The type must be one of the following:
  * feat: A new feature
  * fix: A bug fix
  * docs: Documentation only changes
  * refactor: A code change that neither fixes a bug or adds a feature
  * perf: A code change that improves performance
  * test: Adding missing tests
  * chore: Changes to the build process or auxiliary tools or libraries

### Scope

The scope could be anything specifying the place of the commit change,
such as, `Importer`, `Loader`, `Editor`, etc...

### Subject

The subject contains succinct description of the change.

### Body

The body should include the motivation for
the change, compared with the previous behavior.
References to previous commit hashes is actively encouraged if they are relevant.

**Example:**

~~~
fix(Loader): 65K mesh fully loaded
  
Split objects with more than 65000 vertices or indices into sub-game objects.
  
After parsing the OBJ file, when building meshes as components of game objects in Unity,
an error occurs if exceeding the maximum vertex number for a mesh.
To avoid this split objects with complex meshes into sub-game objects.
~~~

Here the type is `fix`, the scope is `Loader`, the subject is `65K mesh fully loaded` and the remainder is the body (a brief description followed by motivations, remarks, etc.).

## Adding features
If you are going to add new scripts or implement new features please:
* follow the existing folder structure:
  * always put scripts in a `Scripts` folder or in a proper sub-folder
  * put core scripts in `Assets/Scripts` and examples scripts in `Assets/Examples/Scripts`
* if you add 3D models for testing
  * put models used for testing in the `models` folder, outside the `Assets` folder
    (otherwise Unity Editor will import them automatically)
  * put models in the same folder only if they share materials and/or textures

## Submitting Changes
  * Push your changes to your topic branch in your repository.
  * Submit a pull request to the repository `https://github.com/gpvigano/AsImpL`.
  * We will look at the pull request and provide feedback where required.
  * The waiting time for a feedback depends on our availability and the complexity of the issue

### Using SourceTree
Here is a sample workflow with [SourceTree].
  * Getting started (assuming you already forked the AsImplLodr project in GitHub to *mygitaccount*, your Git account):
    * **File|Clone/New**
      * **Source Path/URL:** https://github.com/*mygitaccount*/AsImpL
      * **Destination path:** *(whateverfolderyoulike)*/AsImpL
      * **Clone**
      * wait...
    * **Repository|Repository Settings...**
      * **Add**
        * **Remote name:** upstream *(remember: this is the original repository)*
        * **URL/Path:** https://github.com/gpvigano/AsImpL
        * **OK**
      * **OK**
  * Update your repository (strongly encouraged before making changes)
    * select **master** branch
    * **Repository|Pull...**
      * **Pull from remote:** change to **upstream**
      * **Remote branch to pull:** select **master**
      * **OK**
    * **Repository|Push...**
      * **Push to repository:** select **origin**
      * **Branches to push:** select **master**
      * **Push**
      * wait and you'll get your repository updated on GitHub
  * Make changes
   * select **master** branch
   * **Repository|Branch...**
     * **New name:** *your_new_branch* (e.g. `fix/suspended_on_focus_lost`)
     * **Create Branch**
   * *Make your changes*
   * Select **File Status** tab
     * Stage files (e.g. **Stage All**)
     * Enter a message (see **Commit Messages**, above in this document
       * If you must fix a wrong commit (before pushing to server)
         choose **Amend latest commit** in **Commit options...**
     * **Commit**
   * Push to remote
     * check everything is OK
     * **Repository|Push...**
       * **Push to repository:** select **origin**
       * **Branches to push:** select your branch(e.g. `fix/suspended_on_focus_lost`)
       * if you already pushed your commit to GitHub
         (before your branch has been merged) you can check **Force Push**
         (to enable it select **Tools|Options|Git**,
         check **Enable Force Push**, leaving the option
         `Use Safe Force Push (--force-with-lease)`)
       * **Push**
   * Create a pull request (after pushing your changes)
     * *[SourceTree has a proper interface](https://confluence.atlassian.com/bitbucket/work-with-pull-requests-223220593.html) for this, but here is another way.* Open your Internet browser and browse to your project on GitHub, then press the [button to create a new pull request]
   * Some other hints:
     * [squashing and rebasing](http://blogs.atlassian.com/2014/06/interactive-rebase-sourcetree/) is a way of changing your commit history
       * do not push before squashing commits
       * always check remote repository before rebasing to master
       * do not rebase while editing commit messages (do it in 2 steps)
     * to undo last commit select it in **Log/History**, right click the previous commit and select **Reset current branch to this commit**
     * be careful... ;-)

[Fork]: http://help.github.com/fork-a-repo/
[SourceTree]: https://www.sourcetreeapp.com/
[StyleCop Rules]: http://stylecop.soyuz5.com/Ordering%20Rules.html
[Jonathan Wright]: http://stackoverflow.com/users/28840/jonathan-wright
[StackOverflow]: http://stackoverflow.com/questions/150479/order-of-items-in-classes-fields-properties-constructors-methods
[button to create a new pull request]: https://help.github.com/articles/creating-a-pull-request-from-a-fork/
[Open a Pull Request]: https://help.github.com/articles/using-pull-requests/
[Doxygen]: http://www.doxygen.org/index.html
[AsImpL documentation]: https://github.com/gpvigano/AsImpL/blob/master/Documentation/index.html
