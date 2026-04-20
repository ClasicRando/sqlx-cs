# Benchmarks
## DISCLAIMER
Micro-benchmarks are never great representations of true comparison between libraries/tools. Results
may vary wildly between libraries under different conditions so take all the output below as
cherry-picked outcomes that make `sqlx-cs` look much better than it actually is. To get a good idea
as to how the libraries compare, take some of the main queries your application runs and see how
they perform under similar conditions your applications is under. Then run your system under load
with both libraries to get the true comparison.

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
| Npgsql     | Batched Queries                                | 370.6 us    | 19.20 us    | 55.10 us    | 356.1 us    | 1.02  | 0.21    | -          | -          | -         | 53.48 KB     | 1.00        |
| sqlx-cs-pg | Batched Queries                                | 239.7 us    | 11.55 us    | 32.94 us    | 235.8 us    | 0.66  | 0.13    | -          | -          | -         | 50.87 KB     | 0.95        |
|            |                                                |             |             |             |             |       |         |            |            |           |              |             |
| Npgsql     | Simple Query, All Rows                         | 12,880.9 us | 947.10 us   | 2,747.71 us | 12,087.2 us | 1.04  | 0.30    | 1000.0000  | -          | -         | 20324.52 KB  | 1.00        |
| sqlx-cs-pg | Simple Query, All Rows                         | 11,197.4 us | 1,374.07 us | 4,008.23 us | 12,995.0 us | 1.09  | 0.39    | 1000.0000  | -          | -         | 20342.31 KB  | 1.00        |
|            |                                                |             |             |             |             |       |         |            |            |           |              |             |
| Npgsql     | Simple Query, Concurrent Connections, All Rows | 69,390.3 us | 1,651.31 us | 4,764.39 us | 68,777.7 us | 1.00  | 0.10    | 14000.0000 | 13000.0000 | 2000.0000 | 202915.69 KB | 1.00        |
| sqlx-cs-pg | Simple Query, Concurrent Connections, All Rows | 64,849.7 us | 1,664.55 us | 4,749.05 us | 64,628.6 us | 0.94  | 0.09    | 13000.0000 | 12000.0000 | 1000.0000 | 202916.34 KB | 1.00        |
|            |                                                |             |             |             |             |       |         |            |            |           |              |             |
| Npgsql     | Simple Query, Multi Row                        | 308.5 us    | 10.67 us    | 30.25 us    | 309.5 us    | 1.01  | 0.14    | -          | -          | -         | 48.48 KB     | 1.00        |
| sqlx-cs-pg | Simple Query, Multi Row                        | 214.1 us    | 6.46 us     | 18.23 us    | 216.6 us    | 0.70  | 0.09    | -          | -          | -         | 46.73 KB     | 0.96        |
|            |                                                |             |             |             |             |       |         |            |            |           |              |             |
| Npgsql     | Simple Query, Single Row                       | 236.2 us    | 4.69 us     | 13.00 us    | 234.5 us    | 1.00  | 0.08    | -          | -          | -         | 7.38 KB      | 1.00        |
| sqlx-cs-pg | Simple Query, Single Row                       | 154.9 us    | 4.47 us     | 12.91 us    | 153.4 us    | 0.66  | 0.06    | -          | -          | -         | 6.45 KB      | 0.87        |

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
| Npgsql     | CopyIn, Binary  | 64.76 ms | 1.292 ms | 3.471 ms | 63.90 ms | 1.00  | 0.07    | -         | 1566.91 KB  | 1.00        |
| sqlx-cs-pg | CopyIn, Binary  | 68.79 ms | 1.373 ms | 3.850 ms | 67.25 ms | 1.07  | 0.08    | -         | 1556.12 KB  | 0.99        |
|            |                 |          |          |          |          |       |         |           |             |             |
| Npgsql     | CopyIn, CSV     | 87.39 ms | 1.742 ms | 4.241 ms | 87.08 ms | 1.00  | 0.07    | -         | 4.76 KB     | 1.00        |
| sqlx-cs-pg | CopyIn, CSV     | 83.94 ms | 1.671 ms | 3.488 ms | 82.92 ms | 0.96  | 0.06    | -         | 3.8 KB      | 0.80        |
|            |                 |          |          |          |          |       |         |           |             |             |
| Npgsql     | CopyOut, Binary | 17.00 ms | 1.659 ms | 4.814 ms | 15.38 ms | 1.08  | 0.42    | 1000.0000 | 20290.66 KB | 1.00        |
| sqlx-cs-pg | CopyOut, Binary | 14.72 ms | 1.472 ms | 4.271 ms | 13.80 ms | 0.93  | 0.37    | 1000.0000 | 20298.46 KB | 1.00        |
|            |                 |          |          |          |          |       |         |           |             |             |
| Npgsql     | CopyOut, CSV    | 17.69 ms | 0.544 ms | 1.551 ms | 17.37 ms | 1.01  | 0.12    | -         | 586.63 KB   | 1.00        |
| sqlx-cs-pg | CopyOut, CSV    | 18.11 ms | 0.831 ms | 2.410 ms | 17.19 ms | 1.03  | 0.16    | -         | 199.76 KB   | 0.34        |
