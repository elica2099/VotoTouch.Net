SELECT	A.CoAz, A.IdAzion, A.Sesso,
		CASE WHEN A.FisGiu ='F' THEN A.Cognome+ ' ' + A.Nome ELSE A.Raso END as Raso1,
		isnull(C.IDAzion, -1) as TitIdAzion, isnull(C.NumVotaz, -1) as TitIDVotaz,
		isnull(COALESCE(T.Azioni1Ord,0)+COALESCE(T.Azioni2Ord,0), 0) AS AzOrd,
		isnull(COALESCE(T.Azioni1Str,0)+COALESCE(T.Azioni2Str,0), 0) AS AzStr

FROM GEAS_Titolari AS T with (NOLOCK) INNER JOIN GEAS_Anagrafe As A  with (NOLOCK) ON T.IdAzion = A.IdAzion 
left join VS_conschede AS C WITH (nolock) on A.IDAzion = C.IdAzion and C.NumVotaz = @IDVotaz

WHERE T.Badge = @Badge AND T.Reale=1