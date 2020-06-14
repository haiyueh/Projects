import pandas_gbq
import pandas
from os import listdir
from os.path import isfile, join

# TODO: Set project_id to your Google Cloud Platform project ID.
project_id = "booming-pride-278623"
table_id = "CCDataset.CCTable"


#File path
dataPath = "C:\Git\Projects\Current Cycling\Python\Data"



#Enumerates all files in folder
onlyfiles = [f for f in listdir(dataPath) if isfile(join(dataPath, f))]

#Prints initial progress
print ("Percent Done: 0.0%")

#Loops through the entire folder, adding files
for i, val in enumerate(onlyfiles):
    #Constructs the file path
    filePath = dataPath + "\\" + val

    #Reads the entire CSV file into a pandas dataframe
    data = pandas.read_csv(filePath)

    #Renames the columns on the pandas dataframe to match BigQuery's schema
    data.rename(columns={'Cycle Number':'CycleNumber'}, inplace=True)
    data.rename(columns={'Epoch Time (seconds)':'EpochTime_Seconds'}, inplace=True)
    data.rename(columns={'Total Time (hrs)':'TotalTest_Hours'}, inplace=True)
    data.rename(columns={'Time into Cycle (min)':'MinutesIntoCycle'}, inplace=True)
    data.rename(columns={'Current Status':'CurrentBiasIsOn'}, inplace=True)
    data.rename(columns={'Sample Name':'SampleName'}, inplace=True)
    data.rename(columns={'Current (A)':'Current_Amps'}, inplace=True)
    data.rename(columns={'Voltage (V)':'Voltage_Volts'}, inplace=True)
    data.rename(columns={'Estimated Rs':'EstimatedRs_mOhms'}, inplace=True)
    data.rename(columns={'Temp 1':'Temp1_C'}, inplace=True)
    data.rename(columns={'Temp 2':'Temp2_C'}, inplace=True)
    data.rename(columns={'Temp 3':'Temp3_C'}, inplace=True)
    data.rename(columns={'Temp 4':'Temp4_C'}, inplace=True)
    data.rename(columns={'Temp 5':'Temp5_C'}, inplace=True)
    data.rename(columns={'Temp 6':'Temp6_C'}, inplace=True)
    data.rename(columns={'Temp 7':'Temp7_C'}, inplace=True)
    data.rename(columns={'Temp 8':'Temp8_C'}, inplace=True)
    data.rename(columns={'Temp 9':'Temp9_C'}, inplace=True)
    data.rename(columns={'Temp 10':'Temp10_C'}, inplace=True)
    data.rename(columns={'Temp 11':'Temp11_C'}, inplace=True)
    data.rename(columns={'Temp 12':'Temp12_C'}, inplace=True)
    data.rename(columns={'Temp 13':'Temp13_C'}, inplace=True)
    data.rename(columns={'Temp 14':'Temp14_C'}, inplace=True)
    data.rename(columns={'Temp 15':'Temp15_C'}, inplace=True)
    data.rename(columns={'Temp 16':'Temp16_C'}, inplace=True)
    data.rename(columns={'SmokeVoltage 1':'SmokeLevel1_Volts'}, inplace=True)
    data.rename(columns={'SmokeVoltage 2':'SmokeLevel2_Volts'}, inplace=True)
    data.rename(columns={'SmokeVoltage 3':'SmokeLevel3_Volts'}, inplace=True)
    data.rename(columns={'SmokeVoltage 4':'SmokeLevel4_Volts'}, inplace=True)
    data.rename(columns={'SmokeVoltage 5':'SmokeLevel5_Volts'}, inplace=True)
    data.rename(columns={'SmokeVoltage 6':'SmokeLevel6_Volts'}, inplace=True)
    data.rename(columns={'SmokeVoltage 7':'SmokeLevel7_Volts'}, inplace=True)
    data.rename(columns={'SmokeVoltage 8':'SmokeLevel8_Volts'}, inplace=True)
    data.rename(columns={'SmokeLevel 1':'SmokeVoltage1_Volts'}, inplace=True)
    data.rename(columns={'SmokeLevel 2':'SmokeVoltage2_Volts'}, inplace=True)
    data.rename(columns={'SmokeLevel 3':'SmokeVoltage3_Volts'}, inplace=True)
    data.rename(columns={'SmokeLevel 4':'SmokeVoltage4_Volts'}, inplace=True)
    data.rename(columns={'SmokeLevel 5':'SmokeVoltage5_Volts'}, inplace=True)
    data.rename(columns={'SmokeLevel 6':'SmokeVoltage6_Volts'}, inplace=True)
    data.rename(columns={'SmokeLevel 7':'SmokeVoltage7_Volts'}, inplace=True)
    data.rename(columns={'SmokeLevel 8':'SmokeVoltage8_Volts'}, inplace=True)
    data.rename(columns={'# Cells':'NumCells'}, inplace=True)
    data.rename(columns={'Cell VoC':'CellVoc_Volts'}, inplace=True)
    data.rename(columns={'TempSensor':'TempSensorNumber'}, inplace=True)
    data.rename(columns={'SetCurrent':'SetCurrent_Amps'}, inplace=True)

    #Adds the data to the database
    pandas_gbq.to_gbq(data, table_id, project_id=project_id,if_exists='append')

    #Prints the progress
    print ("Percent Done: " + str(round((i+1)/len(onlyfiles)*100,1)) + "%")



df2 = pandas.DataFrame(
    {
        "LogTimeStamp": pandas.date_range("now", periods=1),
        "SampleName": "C1909-026_1696",
        "CycleNumber": 8242,
        "EpochTimeStamp": 0.000968055555555556,
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
        "TempSensorNumber": 16,
        "SetCurrent_Amps": 10.0
    }
)

