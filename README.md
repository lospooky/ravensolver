# RavenSolver
RavenSolver is a fully automated solver for [Progressive Matrices Problems] (https://en.wikipedia.org/wiki/Raven's_Progressive_Matrices),
a widely used format of abstract reasoning test.<br>
It was developed by me and my partner Victor Ström (now Hagelbäck) as our Master's Thesis project.<br>
In brief, it takes a Progressive Matrix as its input and it will (hopefully) generate the solution.</br>
For a thorough description of the approach we used as well as for the results we obtained on Raven's Standard Progressive Matrices
sections C-D-E, please refer to our [thesis report] (http://studentarbeten.chalmers.se/publication/123536-an-anthropomorphic-solver-for-ravens-progressive-matrices)
or to the [journal article] (http://dx.doi.org/10.1016/j.cogsys.2012.08.002) we published.

## Installation
* Download the latest release or clone the repo, build with Visual Studio 2015
* Use the OneClick Installer (YMMV)

## Usage
* Select a single XAML input file or a directory containing several ones in the **Choose Problem** tab.
* Ravensolver will now process the problem.
* In the **Active Problem** tab you will now see the current problem and a textual description of the found solution.
  * Flagging **Show Solution** will display the computed total or partial solution in the bottom-right matrix cell.
  * **<<** or **>>** will make RavenSolver move to the previous/next problem file, if a directory was selected.
* The **Visualization** tab displays the representation graph RavenSolver has built for the problem. The graph is the basis on which the solution is computed. Have fun with it!! :D
* In **Logger** you will see the processing log for the problem, outlining all the operations RavenSolver performed.

### Problem Files
RavenSolver expects its problem files to be in XAML format. In hindsight this decision hasn't really been futureproof but at the time we found it very convenient.<br>
In the **Matrices** directory, we provided a few sample problem files, including an empty one.<br>
Unfortunately due to copyright reasons we cannot provide the original problem files from the Raven's Standard Progressive Matrices.<br>
XAML is a vector graphics as well as a serialization format largely based on XML, so you should be able to create your own relatively easily with your favorite text editor.


## Credits

&copy; Simone Cirillo, Victor Hagelbäck 2010-2015

## License
RavenSolver is distributed under the MIT License