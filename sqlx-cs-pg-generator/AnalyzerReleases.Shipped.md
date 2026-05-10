
## Release 1.0

### New Rules

| Rule ID   | Category              | Severity | Notes                                                             |
|-----------|-----------------------|----------|-------------------------------------------------------------------|
| SQLxPG001 | sqlx-cs-pg Generation | Error    | Non-partial type                                                  |
| SQLxPG002 | sqlx-cs-pg Generation | Warning  | Annotated type should be s struct but is a ref type               |
| SQLxPG003 | sqlx-cs-pg Generation | Error    | Type is invalid for source generation                             |
| SQLxPG004 | sqlx-cs-pg Generation | Error    | Integer backed enum explicitly chooses another int type           |
| SQLxPG005 | sqlx-cs-pg Generation | Error    | Type for source generation or interception is not a valid DB type |
| SQLxPG006 | sqlx-cs-pg Generation | Error    | More field attributes than expected for property                  |
