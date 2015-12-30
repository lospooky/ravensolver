# RavenSolver

RavenSolver is a fully automated solver for [Progressive Matrices Problems] (https://en.wikipedia.org/wiki/Raven's_Progressive_Matrices),
a widely used format of abstract reasoning test.<br>
It was developed by me and Victor Ström (now Hagelbäck) as our Master's Thesis project.<br>
In brief, it takes a Progressive Matrix as its input and it will (hopefully) produce the solution.<br>

If you're familiar with the RPM, Ravensolver's novelty consists in the fact that it does not pick the solution out of the provided alternatives, but it generates it entirely on its own.
For a thorough description of the approach we used and for the results we obtained on Raven's Standard Progressive Matrices sections C-D-E, please refer to our [thesis report] (http://studentarbeten.chalmers.se/publication/123536-an-anthropomorphic-solver-for-ravens-progressive-matrices)
or to the [journal article] (http://dx.doi.org/10.1016/j.cogsys.2012.08.002) we published.

<p>
Ravensolver was entirely developed in C#, and is intended to run on Windows systems only.<br>
It requires .NET 3.5 or greater.<br>
If you attempt building it on Linux systems, i.e. with Mono, let us know how that goes! 
</p>

## Installation
* [Download] (https://github.com/lospooky/ravensolver/archive/master.zip) the source, build with Visual Studio 2015.
* Clone the repo, `git clone https://github.com/lospooky/ravensolver.git` and build with Visual Studio 2015.
* [Download](https://github.com/lospooky/ravensolver/releases/download/v1.0/ravensolver-v1.0.zip) the compiled binaries, unzip, run
* Use the [OneClick Installer](http://blog.spook.ee/ravensolver/publish.htm) (YMMV)

## Usage
* Select a single XAML input file or a directory containing many in the **Choose Problem** tab.
* Ravensolver will now automagically process the problem.
* In the **Active Problem** tab you will now see the current problem and a textual description of the found solution.
  * Flagging **Show Solution** will display the computed total or partial solution in the bottom-right matrix cell.
  * **<<** or **>>** will make RavenSolver move to the previous/next problem file, if a directory was selected.
* The **Visualization** tab displays the representation graph RavenSolver has built for the problem. The graph is the basis on which the solution is computed. Have fun with it!! :D
* In **Logger** you will see the processing log for the problem, outlining all the operations RavenSolver has performed to obtain the current solution.

### Problem Files
RavenSolver expects its problem files to be in XAML format.<br>
In hindsight this decision hasn't really been futureproof but at the time we found it very convenient.<br>
Unfortunately due to copyright reasons we cannot provide the files for the actual test items from the Raven's Standard Progressive Matrices sets. However in the **Matrices** directory we provided a few sample problems, including an empty one.<br>
[XAML] (https://msdn.microsoft.com/en-us/library/cc189036%28VS.95%29.aspx) is a vector graphics as well as a serialization format largely based on XML, so you should be able to create your own problems relatively easily with your favorite text editor.


## Credits & License
[blog.spook.ee/ravensolver](http://blog.spook.ee/ravensolver)<br>
&copy; Simone Cirillo, Victor Hagelbäck. 2010-2015.<br>
RavenSolver is distributed under the [GNU GPL License](http://www.gnu.org/licenses/gpl-2.0.html).
