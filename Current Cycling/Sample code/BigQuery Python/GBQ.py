import pandas
import pandas_gbq

# TODO: Set project_id to your Google Cloud Platform project ID.
project_id = "astute-engine-277918"

# TODO: Set table_id to the full destination table ID (including the
#       dataset ID).
table_id = 'Test_Dataset.Test_Table2'

df = pandas.DataFrame(
    {
        "my_string": ["d", "e", "f"],
        "my_int64": [1, 2, 3],
        "my_float64": [4.0, 5.0, 6.0],
        "my_bool1": [True, False, True],
        "my_bool2": [False, True, False],
        "my_dates": pandas.date_range("now", periods=3),
    }
)
pandas_gbq.to_gbq(df, table_id, project_id=project_id, credentials=None, if_exists='append')