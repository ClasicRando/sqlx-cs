# Benchmarks
## Queries
### Overview
Benchmarks are run using BenchmarkDotNet and run the same SQL query and deserialization. Init SQL:
```postgresql
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
```postgresql
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
| Npgsql     | Simple Query, All Rows                         | 13,981.6 us | 1,021.00 us | 2,978.30 us | 13,350.6 us | 1.04  | 0.31    | 1000.0000  | -          | -         | 20293.85 KB  | 1.00        |
| sqlx-cs-pg | Simple Query, All Rows                         | 11,720.8 us | 784.24 us   | 2,120.25 us | 11,182.2 us | 0.87  | 0.24    | 1000.0000  | -          | -         | 20537.02 KB  | 1.01        |
|            |                                                |             |             |             |             |       |         |            |            |           |              |             |
| Npgsql     | Simple Query, Concurrent Connections, All Rows | 69,629.5 us | 1,550.88 us | 4,374.29 us | 68,750.7 us | 1.00  | 0.09    | 14000.0000 | 13000.0000 | 2000.0000 | 202919.74 KB | 1.00        |
| sqlx-cs-pg | Simple Query, Concurrent Connections, All Rows | 68,606.8 us | 1,412.10 us | 3,959.68 us | 68,167.8 us | 0.99  | 0.08    | 14000.0000 | 13000.0000 | 2000.0000 | 205255.66 KB | 1.01        |
|            |                                                |             |             |             |             |       |         |            |            |           |              |             |
| Npgsql     | Simple Query, Multi Row                        | 302.9 us    | 9.14 us     | 26.36 us    | 301.9 us    | 1.01  | 0.12    | -          | -          | -         | 48.17 KB     | 1.00        |
| sqlx-cs-pg | Simple Query, Multi Row                        | 279.6 us    | 12.82 us    | 36.78 us    | 267.0 us    | 0.93  | 0.15    | -          | -          | -         | 47.18 KB     | 0.98        |
|            |                                                |             |             |             |             |       |         |            |            |           |              |             |
| Npgsql     | Simple Query, Single Row                       | 261.6 us    | 9.19 us     | 26.53 us    | 257.1 us    | 1.01  | 0.14    | -          | -          | -         | 7.07 KB      | 1.00        |
| sqlx-cs-pg | Simple Query, Single Row                       | 175.4 us    | 5.60 us     | 15.89 us    | 171.8 us    | 0.68  | 0.09    | -          | -          | -         | 6.43 KB      | 0.91        |
## PostgreSQL COPY
### Overview
Benchmarks are run using BenchmarkDotNet and run the same SQL query and copy data. Init SQL:
```postgresql
DROP TABLE IF EXISTS public.copy_target;
CREATE TABLE public.copy_target(
    id int primary key,
    text_field text not null,
    creation_date timestamp not null,
    last_change_date timestamp not null,
    counter int
);
```
Queries executed during benchmarks:
```postgresql
COPY public.copy_target FROM STDIN WITH (FORMAT CSV);

COPY public.copy_target FROM STDIN WITH (FORMAT binary);
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