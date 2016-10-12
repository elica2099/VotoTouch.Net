
DECLARE @ProgMozione as int
DECLARE @ApVotDate as datetime
DECLARE @ValAssem as VarChar(3)

set @ProgMozione = (select isnull(CONFIG_AppoggioR.ProgMozione, 0) as ProgMozione from CONFIG_AppoggioR)
set @ApVotDate = (select isnull(CONFIG_AppoggioR.ApVotDate, 0) as ApVotDate from CONFIG_AppoggioR)
set @ValAssem = (select isnull(CONFIG_AppoggioR.TipoAssemblea, 0) as TipoAssemblea from CONFIG_AppoggioR)

if @ValAssem = 'O'
begin
	set @ValAssem = 'S'
end
else
begin
	set @ValAssem = 'O'	
end

execute SP_BadgePresente @ApVotDate, @Badge, @ValAssem
