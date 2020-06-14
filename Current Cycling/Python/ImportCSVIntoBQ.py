import pandas_gbq
import pandas

# TODO: Set project_id to your Google Cloud Platform project ID.
project_id = "booming-pride-278623"
table_id = "CCDataset.CCTable"

df2 = pandas.DataFrame(
    {
        "LogTimeStamp": pandas.date_range("now", periods=1),
        "SampleName": "C1909-026_1696",
        "CycleNumber": 8242,
        "EpochTimeStamp": pandas.date_range("now", periods=1),
        "TotalTest_Hours": 47.3811172222222,
        "MinutesIntoCycle": 6.98205,
        "CurrentBias": True,
        "Current_Amps": 10.0,
        "Voltage_Volts": 38.873,
        "EstimatedRs_mOhms": -99.99,
        "Temp1_C": 80.0,
        "Temp2_C": 27.25,
        "Temp3_C": 82.0,
        "Temp4_C": 23.5,
        "Temp5_C": 27.0,
        "Temp6_C": 25.0,
        "Temp7_C": 27.75,
        "Temp8_C": 26.75,
        "Temp9_C": 31.0,
        "Temp10_C": 22.75,
        "Temp11_C": 22.5,
        "Temp12_C": 83.5,
        "Temp13_C": 92.25,
        "Temp14_C": 105.0,
        "Temp15_C": 100.25,
        "Temp16_C": 123.5,
        "SmokeLevel1_Volts": 0.03,
        "SmokeLevel2_Volts": 0.01,
        "SmokeLevel3_Volts": 0.0,
        "SmokeLevel4_Volts": 0.01,
        "SmokeLevel5_Volts": 0.0,
        "SmokeLevel6_Volts": 0.0,
        "SmokeLevel7_Volts": 0.01,
        "SmokeLevel8_Volts": 0.02,
        "SmokeVoltage1_Volts": 0.17,
        "SmokeVoltage2_Volts": 0.17,
        "SmokeVoltage3_Volts": 0.06,
        "SmokeVoltage4_Volts": 0.03,
        "SmokeVoltage5_Volts": 0.03,
        "SmokeVoltage6_Volts": 0.06,
        "SmokeVoltage7_Volts": 0.08,
        "SmokeVoltage8_Volts": 0.08,
        "NumCells": 22,
        "CellVoc_Volts": 0.655,
        "TempSensor_C": 16.0,
        "SetCurrent_Amps": 10.0
    }
)

pandas_gbq.to_gbq(df2, table_id, project_id=project_id,if_exists='append')