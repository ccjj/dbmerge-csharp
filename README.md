# dbmerge-csharp
merges two special access-databases

Merges two special access-databases into the partent database. Every entry in the database will be associated with a date gotten from a sql-cross-table-query
and "clustered". If several entries with the same date follow each other, we are sure that this cluster was created at the same time and entries before
(especially including the ID) are most likely changed later. Clusters with following dates are collected, and the entries between each cluster are given
the date of the upper-cluster. Then the merging starts, if one entry is a duplicate in both databases, we check the internal applied
cluster-date for deciding which one to update.

Its over a year old and just works by now for two very specific databases, might do some things differently today (comments...).
