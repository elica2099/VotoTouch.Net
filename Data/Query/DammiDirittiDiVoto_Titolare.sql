SELECT	A.CoAz, A.IdAzion, A.Sesso, isnull(A.CoFi, '') as CoFi,
		CASE WHEN A.FisGiu ='F' THEN A.Cognome+ ' ' + A.Nome ELSE A.Raso END as Raso1,
		isnull(C.IDAzion, -1) as TitIdAzion, isnull(C.NumVotaz, -1) as TitIDVotaz,
		isnull(T.Voti1Ord,0) as VtOrd1, isnull(T.Voti2Ord,0) as VtOrd2,
		isnull(T.Voti1Str,0) as VtStr1, isnull(T.Voti2Str,0) as VtStr2,
		isnull(P.IdTipoScheda, 99) as AK_PrevVote

FROM GEAS_Titolari AS T with (NOLOCK) INNER JOIN GEAS_Anagrafe As A  with (NOLOCK) ON T.IdAzion = A.IdAzion 
left join VS_conschede AS C WITH (nolock) on A.IDAzion = C.IdAzion and C.NumVotaz = @IDVotaz
left join VS_AK_VotiPrecedenti P on A.cofi = P.cofi

WHERE T.Badge = @Badge AND T.Reale=1