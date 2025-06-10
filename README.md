# PipeFlow

Implémentation d’un enchainement d’action en .Net

## Entités

**Connector**

- Elément générique prenant un TEntree en entré en le transformant en TSortie en sortie de processus

**Builder**

- Constructeur permettant l’enchainement des Connector en respectant les types d’entrée et de sortie
- Génère un PipeBuilderDetails<TEntree, TSortie> décrivant les étapes a parcourir

**Runner**

- Lance le processus décrit dans un PipeBuilderDetails<TEntree, TSortie> ainsi qu’un Input d’entrée pour en calculer le résultat