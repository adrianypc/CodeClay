select * from CreditNote where CreditNoteNo = 'HCN0001307'
select * from CreditNoteItem where CreditNoteNo = 'HCN0001307'
select * from Payment where PaymentBillNo =  'HCN0001307'
select * from ClientBill where ClientBillNo = 'HCC0130257'
select * from ClientBillItem where ClientBillNo = 'HCC0130257'
select * from Payment where PaymentBillNo =   'HCC0130257'



select GL.AccountDescription,AE.* from AccountEntry AE
inner join GLAccount GL
on AE.AEAccountCode = GL.AccountCode
where TransactionCode = 'HCN0001307'

select * from AccountEntry where AEAccountCode = '2001/000' and AEAmount > 0

---select top 1 * from Appointment;

select top 100 * from Appointment where AppointmentIdentityNo <> 'Busy' order by AppointmentDate desc
select Convert(datetime,Convert(varchar(11),DateADD(day,getdate(),1),103),103) 
