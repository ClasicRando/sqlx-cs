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

| Method     | Categories                                     |        Mean |       Error |      StdDev |      Median | Ratio | RatioSD |       Gen0 |       Gen1 |      Gen2 |    Allocated | Alloc Ratio |
|------------|------------------------------------------------|-------------|-------------|-------------|-------------|-------|---------|------------|------------|-----------|--------------|-------------|
| Npgsql     | Simple Query, All Rows                         | 13,599.6 us |   969.47 us | 2,797.15 us | 13,273.8 us |  1.04 |    0.30 |  1000.0000 |          - |         - |  20294.88 KB |        1.00 |
| sqlx-cs-pg | Simple Query, All Rows                         | 12,030.5 us |   906.46 us | 2,496.64 us | 11,211.3 us |  0.92 |    0.27 |  1000.0000 |          - |         - |   20526.8 KB |        1.01 |
|            |                                                |             |             |             |             |       |         |            |            |           |              |             |
| Npgsql     | Simple Query, Concurrent Connections, All Rows | 70,496.7 us | 1,406.13 us | 3,825.49 us | 70,294.1 us |  1.00 |    0.08 | 14000.0000 | 13000.0000 | 2000.0000 | 202918.61 KB |        1.00 |
| sqlx-cs-pg | Simple Query, Concurrent Connections, All Rows | 71,108.5 us | 1,555.74 us | 4,310.95 us | 70,949.5 us |  1.01 |    0.08 | 14000.0000 | 13000.0000 | 2000.0000 | 205266.19 KB |        1.01 |
|            |                                                |             |             |             |             |       |         |            |            |           |              |             |
| Npgsql     | Simple Query, Multi Row                        |    313.8 us |    13.87 us |    39.81 us |    308.8 us |  1.02 |    0.18 |          - |          - |         - |     48.48 KB |        1.00 |
| sqlx-cs-pg | Simple Query, Multi Row                        |    244.9 us |     8.86 us |    25.55 us |    244.8 us |  0.79 |    0.13 |          - |          - |         - |     47.82 KB |        0.99 |
|            |                                                |             |             |             |             |       |         |            |            |           |              |             |
| Npgsql     | Simple Query, Single Row                       |    277.0 us |     9.22 us |    26.16 us |    272.6 us |  1.01 |    0.13 |          - |          - |         - |      7.38 KB |        1.00 |
| sqlx-cs-pg | Simple Query, Single Row                       |    158.3 us |     4.62 us |    13.40 us |    154.3 us |  0.58 |    0.07 |          - |          - |         - |      6.98 KB |        0.95 |

## COPY
### Overview
TODO

### Results
TODO
