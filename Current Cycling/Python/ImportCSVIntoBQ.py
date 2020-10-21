import pandas_gbq
import pandas
from os import listdir
from os.path import isfile, join
from google.oauth2 import service_account
import json

project_id = "booming-pride-278623"
table_id = "CCDataset.CCTable"
with open('creds.txt') as json_file:
    creds = json.load(json_file)
credentials = service_account.Credentials.from_service_account_info(creds)


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
    data.rename(columns={'Epoch Time (seconds)':'LogTime_Timestamp'}, inplace=True)
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
    data.rename(columns={'SmokeVoltage 1':'SmokeVoltage1_Volts'}, inplace=True)
    data.rename(columns={'SmokeVoltage 2':'SmokeVoltage2_Volts'}, inplace=True)
    data.rename(columns={'SmokeVoltage 3':'SmokeVoltage3_Volts'}, inplace=True)
    data.rename(columns={'SmokeVoltage 4':'SmokeVoltage4_Volts'}, inplace=True)
    data.rename(columns={'SmokeVoltage 5':'SmokeVoltage5_Volts'}, inplace=True)
    data.rename(columns={'SmokeVoltage 6':'SmokeVoltage6_Volts'}, inplace=True)
    data.rename(columns={'SmokeVoltage 7':'SmokeVoltage7_Volts'}, inplace=True)
    data.rename(columns={'SmokeVoltage 8':'SmokeVoltage8_Volts'}, inplace=True)
    data.rename(columns={'SmokeLevel 1':'SmokeLevel1_Volts'}, inplace=True)
    data.rename(columns={'SmokeLevel 2':'SmokeLevel2_Volts'}, inplace=True)
    data.rename(columns={'SmokeLevel 3':'SmokeLevel3_Volts'}, inplace=True)
    data.rename(columns={'SmokeLevel 4':'SmokeLevel4_Volts'}, inplace=True)
    data.rename(columns={'SmokeLevel 5':'SmokeLevel5_Volts'}, inplace=True)
    data.rename(columns={'SmokeLevel 6':'SmokeLevel6_Volts'}, inplace=True)
    data.rename(columns={'SmokeLevel 7':'SmokeLevel7_Volts'}, inplace=True)
    data.rename(columns={'SmokeLevel 8':'SmokeLevel8_Volts'}, inplace=True)
    data.rename(columns={'# Cells':'NumCells'}, inplace=True)
    data.rename(columns={'Cell VoC':'CellVoc_Volts'}, inplace=True)
    data.rename(columns={'TempSensor':'TempSensorNumber'}, inplace=True)
    data.rename(columns={'SetCurrent':'SetCurrent_Amps'}, inplace=True)

    #Casts SetCurrent_Amps to float
    data['SetCurrent_Amps'] = data['SetCurrent_Amps'].astype(float)

    #Casts Logtime from Epoch to Datetime
    data['LogTime_Timestamp'] = pandas.to_datetime(data['LogTime_Timestamp'],unit='s')

    #Adds the data to the database
    pandas_gbq.to_gbq(data, table_id, project_id=project_id,if_exists='append',credentials=credentials)

    #Prints the progress
    print ("Percent Done: " + str(round((i+1)/len(onlyfiles)*100,1)) + "%")


