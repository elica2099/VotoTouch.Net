SELECT	A.CoAz, A.IdAzion, A.Sesso,
		CASE WHEN A.FisGiu ='F' THEN A.Cognome+ ' ' + A.Nome ELSE A.Raso END as Raso1,
		isnull(VS_conschede.IDAzion, -1) as TitIdAzion

FROM GEAS_Titolari AS T with (NOLOCK) INNER JOIN GEAS_Anagrafe As A  with (NOLOCK) ON T.IdAzion = A.IdAzion 
left join VS_conschede WITH (nolock) on A.IDAzion = VS_conschede.IdAzion 

WHERE T.Badge = @Badge AND T.Reale=1