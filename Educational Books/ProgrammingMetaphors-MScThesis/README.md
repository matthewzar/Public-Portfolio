# Abstract

It is well established that both learning and teaching programming are difficult tasks. Difficulties often occur due to weak mental models and common misconceptions. This study proposes a method of teaching programming that both encourages high-fidelity mental models and attempts to minimise misconceptions in novice programmers, through the use of metaphors and manipulatives. The elements in ActionWorld with which the students interact are realizations of metaphors. By simple example, a variable has a metaphorical representation as a labeled box that can hold a value. The dissertation develops a set of metaphors which have several core requirements: metaphors should avoid causing misconceptions, they need to be high-fidelity so as to avoid failing when used with a new concept, students must be able to relate to them, and finally, they should be usable across multiple educational media.

The learning style that ActionWorld supports is one which requires active participation from the student - the system acts as a foundation upon which students are encouraged to build their mental models. This teaching style is achieved by placing the student in the role of code interpreter, the code they need to interpret will not advance until they have demonstrated its meaning via use of the aforementioned metaphors.

ActionWorld was developed using an iterative developmental process that consistently improved upon various aspects of the project through a continual evaluation-enhancement cycle.

The primary outputs of this project include a unified set of high-fidelity metaphors, a virtual-machine API for use in similar future projects, and two metaphor-testing games. All of the aforementioned deliverables were tested using multiple quality-evaluation criteria, the results of which were consistently positive. ActionWorld and its constituent components contribute to the wide assortment of methods one might use to teach novice programmers.

## Future work

This paper was awarded a distinction (75% or more), and goes into a lot of detail around how one might overcome threshold concepts when teaching novices to program. However, it asks a lot of questions, and suggests many future extensions/projects, including but not limited to:
 - A more engaging or visually appealing game world.
 - Migration from XNA and WPF to a more modern game framework.
 - Use of proposed metaphors to actually teach students, and evaluate their responses. This wasn't done due to time constraint, but it would also require ethical clearance.
 - Extending the metaphors to cover more advanced programming concepts. Such as abstractions, dependency injection, or reflection. 
 - Evaluating at what level of complexity the metaphors begin to break down.

## [Associated code](https://github.com/matthewzar/metaphor-world-msc-thesis)

As one might expect, the code associated with this project was written by a student (who has since learned **a lot** about style, architecture, CI, and design patterns). Therefore, while it can be used as inspiration, it should not be used as the foundation for a similar project. 

It should also not be used to judge the current development abilities of the author.

Certain isolated sub-components (while not pretty) should be re-usable, such as the reflection-based calculator that invokes arbitrary C# code at runtime.