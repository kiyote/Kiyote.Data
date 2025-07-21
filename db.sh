set -eu

invokesql() {
  /opt/mssql-tools18/bin/sqlcmd \
  -S 127.0.0.1,1433 \
  -U SA \
  -P $MSSQL_SA_PASSWORD \
  "$@"
}

counter=1
errstatus=1
while [ $counter -le 5 ] && [ $errstatus = 1 ]
do
  set +e # expect errors for a little bit
  echo "Waiting for SQL Server to start..."
  sleep 5s
  invokesql -Q "SELECT @@VERSION"
  errstatus=$?
  ((counter++))
done

set -e # no more errors

if [ $errstatus = 1 ]
then
  echo "Cannot connect to SQL Server, installation aborted"
  exit $errstatus
fi
