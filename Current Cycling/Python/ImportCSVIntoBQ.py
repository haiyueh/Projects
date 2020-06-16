import pandas_gbq
import pandas
from os import listdir
from os.path import isfile, join
from google.oauth2 import service_account


project_id = "booming-pride-278623"
table_id = "CCDataset.CCTable"
credentials = service_account.Credentials.from_service_account_info(
{
  "type": "service_account",
  "project_id": "booming-pride-278623",
  "private_key_id": "85349bff3fab73238ad054fa4971007a43fb4c8e",
  "private_key": "-----BEGIN PRIVATE KEY-----\nMIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDTTF6mRmM6HbeK\ntzstnkzn4Bo6GBBJyuCGVLBQmRZh2QUATyuImkL15gIWDSyGeq/xpx5IgQyAvk0A\nEILs4qymvLG4SkSjvyBZ4CMBsTT9BPepH1+DN6QQDRxWAspi2Ous4Bs4l0n3Zpaz\n8pgrAuuxF3yHcaqdGsn5ZHbbuud421uOIoOvMwymOkoT38dA1oWubFonmsQH/G7l\nT+YLarWXcnMvsRdH6PEXv3N8+VIVZ0eYwLXu0dTtgpumd8HQqI14V0Ltilutt5QU\nMGqp4q8b8paeAWXF8kCdOaGiP6FiECPuYMqxLcp+DDfWqivID1Qnx0lyoaQQKYU4\n7pmbiKW1AgMBAAECggEADxFfoIjdc5wB5jT51d93iYOMKz9jDfgXOc0ZQ64fD/w7\nZJHAv32Mr99mn67x0Wc3W8q4mKAHRbkMYahLxdvGRx+mpsc5DNpI0s/ufyTla/Oq\nO5e+pmtV2kUtE58gUps4dzwGBOuY/TkGHy/6FPsg1qCfo0MCUTDDXXMCHNo/vnj4\n5ddi4HZqInfWEhBkqBG0UDf2eUGBEycPXxcFWUMAeQrr38BPxK3FPquq2Ss27v57\ny94jtv9RKz6KhwBMGPk+xHSXyxOm8Z9EpNickspgH4xDY5c05DQ7WjffbA3xC5CL\nl6hZ9OxL3AD/e5qHgl+yEdiCA4JaRcWpvc3P0/vFMQKBgQDw7/vKY61VqY/lAcGe\nbZu/Ex5prfXP7L++h/O4MRvKUD35YHAajLpoY/ea135+4OHe/liX6Hg1KoQGDvF0\n14DzOJg2LaTcgb5UFv4zMAZA0Ga2rsdBaDGFw0mGN3kBAFEYGsaLCS/NlrLq7LSC\nJXZT7c839KSypyTnpvBzZytC3QKBgQDgggj+0LJMQrMTKEbtxxOk5Ur/jeK5S3X0\n9u7gBtKeCkyABG6dh4Zmf08k7qiH4B1KBLRxxXALzktd4aW8uxLbNBYhqkM4Dmjs\n8OJ+R9M7qHRL9mzPy6fwsyS9c4UwovQIxnyNpL8WqOkzPs704Gb3GpPck6HcZX0K\ntxLnB9NkuQKBgQDuIAF28jTqKP+ykp3N+v6nRjoUsH311kNcB/n03XRd7BiUU5/4\nXYYOjl61hq3asGAMiMz+th+4TCDX7ATwOd2UhSbKxSnfVcvKSD9MT/aeMFqTywHb\nvyLS1UPhhwns12dOr4fy+k1on7yNOwzcZDIimTLoVr5AY7mxyehz5k93cQKBgQDB\nkTwiP1vLBqMRPGPTNRaJ0KxWJEY7zoUYPSN+AkPrwSNuKOQabDQEAXYCeMbTx/ZY\n0C+n/Dv74dT3T8svKvg6CPGf+wXTuhDbYWFW0aSdRkNnD0OH8aaNkFd4BLbsVUMk\nocXX9hhPeDkAVwHm/eeo28BqqNsghFxINcpVaVjo+QKBgB7JPRwXLJxIk2WW5Uks\naO9rG8WVXHjhVetHrthng7je0cwwn1VPXFR28HkHeqaMsUK53Bj1VMWI+0Mp5DHP\nLeh5YbVbQPFtzr06zcTopoQWfozsgEt9arjItW2sMlKLMDSThltoNgVui7U2M5t5\nNeVQPJO378L2gGLy6FTjfhj/\n-----END PRIVATE KEY-----\n",
  "client_email": "c-sharp@booming-pride-278623.iam.gserviceaccount.com",
  "client_id": "106487000463359952131",
  "auth_uri": "https://accounts.google.com/o/oauth2/auth",
  "token_uri": "https://oauth2.googleapis.com/token",
  "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
  "client_x509_cert_url": "https://www.googleapis.com/robot/v1/metadata/x509/c-sharp%40booming-pride-278623.iam.gserviceaccount.com"
}
)


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

    #Casts SetCurrent_Amps to float
    data['SetCurrent_Amps'] = data['SetCurrent_Amps'].astype(float)

    #Casts Logtime from Epoch to Datetime
    data['LogTime_Timestamp'] = pandas.to_datetime(data['LogTime_Timestamp'],unit='s')

    #Adds the data to the database
    pandas_gbq.to_gbq(data, table_id, project_id=project_id,if_exists='append',credentials=credentials)

    #Prints the progress
    print ("Percent Done: " + str(round((i+1)/len(onlyfiles)*100,1)) + "%")


