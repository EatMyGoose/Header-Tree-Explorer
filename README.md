# Header Tree Explorer

A tool to visualize the header dependencies within a C++ project, to help guide choices towards improving compilation times.

Consists of the following tools:
## Header Include Frequency
- Shows the number of times each each file is included, either directly or indirectly (by means of being included by another header), within a C++ project
- Mainly serves as a tool to help decide which files to include within the project's precompiled header

## Header Include Graph
- Generates a .dot/.gv file which gives a visual representation of the #include graph within the entire project
- Used in conjunction with tools such as Graphviz to produce charts such as the following:
![SimpleProject](https://user-images.githubusercontent.com/47314429/159167748-53a1525d-6254-4c60-995b-05e452413cb0.png)
*Header Include Graph for a simple project, rendered using [Graphviz Online](https://dreampuf.github.io/GraphvizOnline/)*
![LargerGraph](https://user-images.githubusercontent.com/47314429/159167943-84dc4976-c3d3-4df8-a185-fd9b4c18ce1d.PNG)
*Portion of a larger header include graph*

- Mainly targeted at helping identify key files which contribute significantly to compilation times (i.e. by virtue of being included multiple times & possessing large #include trees by itself) 

## Usage
- Specify the header files within your project either by using the *Load* button or by specifying the path to a VS C++ project
- (Optional) if the header trees for libraries/external includes are required, specify their directory paths under the *Configure* section
- Select the desired report type (Include Frequency/Header Graph) and generate the report.
- For header include graphs, load the resultant .gv file into a .dot/.gv viewer such as Graphviz/[Graphviz online](https://dreampuf.github.io/GraphvizOnline/)
