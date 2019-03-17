# CodeFactory

This is a showcase of a language abstraction framework for code generation. This was created as a proof of concept to demonstrate the benefits of having an abstraction that can be extented to different languages. 

The main advantages:
* *Scalability*: It's very easy to support a new language. In fact, the final version of the framework supports code generation for C#, Java and JavaScript;
* *Testability*: Having a testing suit for the framework means that the generators only need to test the generated semantics;

## Getting Started

This PoC has three projects: 
* `CodeFactoryPoC` which is the actual framework;
* `ControllerGenerator` a small example of how `CodeFactoryPoC` can be used;
* `CodeFactoryDemo` a demonstration that the generated controller from `ControllerGenerator` actually works.

To use the framework, you simple need to build `CodeFactoryPoC` and then reference it.

## Disclaimer

This is simply a proof of concept. It lacks much needed documentation for the framewok's use (such as comments on each method and an explanation of the `Expression` and `Statement` interactions), tests and more concern for the generate code's style.

## Author

**Henrique Henriques**
