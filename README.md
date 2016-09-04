# CatFlap

Makes Entity Framework suck less. Draws together the concept of projection (Project<T>().To<T>()) as it appears in AutoMapper with some funky expression rewriting to allow filtered collections of related entities to be retreived in a single query and to allow query results to be limited implicitly to the data needed to populate a given viewmodel.

## I don't believe you

