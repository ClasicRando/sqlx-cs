# Benchmarks
## Queries
### Overview
Benchmarks are run using BenchmarkDotNet and run the same SQL query and deserialization. Init SQL:
```sql
DROP TABLE IF EXISTS public.posts;
CREATE TABLE public.posts (
    id int primary key generated always as identity, 
    text_field text not null, 
    creation_date timestamp not null,
    last_change_date timestamp not null,
    counter int
);

INSERT INTO public.posts(text_field, creation_date, last_change_date)
SELECT REPEAT('x', 2000), current_timestamp, current_timestamp
FROM generate_series(1, 5000) s
```
Queries executed during benchmarks:
```sql
-- Single row
SELECT id, text_field, creation_date, last_change_date, counter
FROM public.posts
WHERE id = $1;

-- Multi row
SELECT id, text_field, creation_date, last_change_date, counter
FROM public.posts
WHERE id BETWEEN $1 AND $2;

-- All rows
SELECT id, text_field, creation_date, last_change_date, counter
FROM public.posts;
```

### Results
Results seen below are from a single run so the stats can vary due to IO. However, the general trend
is minimal difference between the 2 drivers.

| Method     | Categories                                     | Mean        | Error       | StdDev      | Median      | Ratio | RatioSD | Gen0       | Gen1       | Gen2      | Allocated    | Alloc Ratio |
|------------|------------------------------------------------|-------------|-------------|-------------|-------------|-------|---------|------------|------------|-----------|--------------|-------------|
| Npgsql     | Batched Queries                                | 337.9 us    | 13.53 us    | 38.83 us    | 330.0 us    | 1.01  | 0.16    | -          | -          | -         | 53.48 KB     | 1.00        |
| sqlx-cs-pg | Batched Queries                                | 254.8 us    | 9.54 us     | 27.05 us    | 251.8 us    | 0.76  | 0.12    | -          | -          | -         | 51.4 KB      | 0.96        |
|            |                                                |             |             |             |             |       |         |            |            |           |              |             |
| Npgsql     | Simple Query, All Rows                         | 13,671.0 us | 780.23 us   | 2,263.58 us | 13,259.5 us | 1.03  | 0.24    | 1000.0000  | -          | -         | 20309.25 KB  | 1.00        |
| sqlx-cs-pg | Simple Query, All Rows                         | 12,771.2 us | 1,148.92 us | 3,183.64 us | 11,856.5 us | 0.96  | 0.29    | 1000.0000  | -          | -         | 20528.64 KB  | 1.01        |
|            |                                                |             |             |             |             |       |         |            |            |           |              |             |
| Npgsql     | Simple Query, Concurrent Connections, All Rows | 70,377.9 us | 1,406.02 us | 3,679.30 us | 70,570.3 us | 1.00  | 0.07    | 14000.0000 | 13000.0000 | 2000.0000 | 202920.96 KB | 1.00        |
| sqlx-cs-pg | Simple Query, Concurrent Connections, All Rows | 69,889.5 us | 1,389.41 us | 3,660.27 us | 69,462.0 us | 1.00  | 0.07    | 13000.0000 | 12000.0000 | 1000.0000 | 205268.41 KB | 1.01        |
|            |                                                |             |             |             |             |       |         |            |            |           |              |             |
| Npgsql     | Simple Query, Multi Row                        | 327.4 us    | 10.10 us    | 29.30 us    | 326.2 us    | 1.01  | 0.13    | -          | -          | -         | 48.48 KB     | 1.00        |
| sqlx-cs-pg | Simple Query, Multi Row                        | 229.2 us    | 7.59 us     | 21.65 us    | 231.0 us    | 0.71  | 0.09    | -          | -          | -         | 47.18 KB     | 0.97        |
|            |                                                |             |             |             |             |       |         |            |            |           |              |             |
| Npgsql     | Simple Query, Single Row                       | 239.2 us    | 6.59 us     | 18.37 us    | 235.6 us    | 1.01  | 0.11    | -          | -          | -         | 7.38 KB      | 1.00        |
| sqlx-cs-pg | Simple Query, Single Row                       | 155.6 us    | 3.93 us     | 11.35 us    | 154.2 us    | 0.65  | 0.07    | -          | -          | -         | 6.43 KB      | 0.87        |
## PostgreSQL COPY
### Overview
Benchmarks are run using BenchmarkDotNet and run the same SQL query and copy data. Init SQL:
```sql
DROP TABLE IF EXISTS public.copy_target;
CREATE TABLE public.copy_target(
    id int primary key,
    text_field text not null,
    creation_date timestamp not null,
    last_change_date timestamp not null,
    counter int
);

DROP TABLE IF EXISTS public.copy_source;
CREATE TABLE public.copy_source(
    id int primary key,
    text_field text not null,
    creation_date timestamp not null,
    last_change_date timestamp not null,
    counter int
);

INSERT INTO public.copy_source(id, text_field, creation_date, last_change_date)
SELECT s.a, REPEAT('x', 2000), current_timestamp, current_timestamp
FROM generate_series(1, 5000) AS s(a);
```
Queries executed during benchmarks:
```sql
COPY public.copy_target FROM STDIN WITH (FORMAT CSV);

COPY public.copy_target FROM STDIN WITH (FORMAT binary);

COPY public.copy_target TO STDOUT WITH (FORMAT CSV);

COPY public.copy_target TO STDOUT WITH (FORMAT binary);
```

### Results
| Method     | Categories      | Mean     | Error    | StdDev   | Median   | Ratio | RatioSD | Gen0      | Allocated   | Alloc Ratio |
|------------|-----------------|----------|----------|----------|----------|-------|---------|-----------|-------------|-------------|
| Npgsql     | CopyIn, Binary  | 66.76 ms | 1.500 ms | 4.350 ms | 65.93 ms | 1.00  | 0.09    | -         | 1566.61 KB  | 1.00        |
| sqlx-cs-pg | CopyIn, Binary  | 69.67 ms | 1.381 ms | 3.564 ms | 68.08 ms | 1.05  | 0.08    | -         | 1572.19 KB  | 1.00        |
|            |                 |          |          |          |          |       |         |           |             |             |
| Npgsql     | CopyIn, CSV     | 86.91 ms | 1.734 ms | 4.350 ms | 87.04 ms | 1.00  | 0.07    | -         | 4.76 KB     | 1.00        |
| sqlx-cs-pg | CopyIn, CSV     | 87.62 ms | 1.748 ms | 4.544 ms | 87.39 ms | 1.01  | 0.07    | -         | 3.81 KB     | 0.80        |
|            |                 |          |          |          |          |       |         |           |             |             |
| Npgsql     | CopyOut, Binary | 15.44 ms | 2.086 ms | 6.118 ms | 12.44 ms | 1.14  | 0.60    | 1000.0000 | 20300.69 KB | 1.00        |
| sqlx-cs-pg | CopyOut, Binary | 14.61 ms | 1.432 ms | 4.154 ms | 13.24 ms | 1.08  | 0.47    | 1000.0000 | 20530.5 KB  | 1.01        |
|            |                 |          |          |          |          |       |         |           |             |             |
| Npgsql     | CopyOut, CSV    | 17.81 ms | 0.679 ms | 1.948 ms | 17.22 ms | 1.01  | 0.15    | -         | 526.79 KB   | 1.00        |
| sqlx-cs-pg | CopyOut, CSV    | 18.65 ms | 0.827 ms | 2.425 ms | 17.77 ms | 1.06  | 0.17    | -         | 117.51 KB   | 0.22        |