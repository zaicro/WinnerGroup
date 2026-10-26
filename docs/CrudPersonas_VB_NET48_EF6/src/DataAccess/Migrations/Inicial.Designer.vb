Imports System.Data.Entity.Migrations.Infrastructure

Namespace Migrations
    Public Partial Class Inicial
        Implements IMigrationMetadata

        Private ReadOnly Property MigrationId As String Implements IMigrationMetadata.Id
            Get
                Return "202610240001000_Inicial"
            End Get
        End Property

        Private ReadOnly Property SourceModel As String Implements IMigrationMetadata.Source
            Get
                Return Nothing
            End Get
        End Property

        Private ReadOnly Property TargetModel As String Implements IMigrationMetadata.Target
            Get
                Return "H4sIAPr73GoC/7VVy27bMBC85ysI3iPaDoq2hqygcOOiaB0EUdI7La1tohSpknRg/32XelCy4qRu0N4o7nI5Ozscxdf7QpInMFZoNaPjaEQJqEznQm1mdOfWlx/odXIR3+TFnvxo0658Gh5Udka3zpVTxmy2hYLbqBCZ0VavXZTpgvFcs8lo9JGNxwywBE0uSHy/U04UgEsSz7XKoHQ7Lpc6B2n9JonTqhi55QXYkmcwo3OMLoSxLuxR8kkKjgBSkGtKuFLacYfwpo8WUme02qQlbnD5cCgB89ZcWmhgT7v0czsYTXwHrDv4JgZo1SCJb5ACd/DIqi5n9A651YrTJP4GhyS+M7oE4w73sG4SvuaUsCRmR+FezNfClXJXE0pud1LylYTQdY+e1GkDX0CB4Q7yO+4cGOVrQIWpumVQ/1YXKwPtHUguqoOSJd9/B7VxW9TNOxTEQuwhb3eaex+VQDHhIWd2cAKX76jj4ogclIbjAmE2IIIEQgDJqlNTcMc0Wkq6qrVEopbh3p2hUi07VuuuEiY7pcx4ycsSe29k2nyRtNHoZfr3mijqGiyzoZuAKtTHifENDKI9Rj5zx1fc8znPi5fTThLXXjLgL+lJtE3x68EswnOMOgE36QvEXKCmKvgQ7utLPc245OaEmOda7gp1JPyTua0w+/ntnh/zAMqR2JpY2Ou4eK6PLtCuKo305RA3UzrTx9qR1SmUYGNPIvfjSg/WQRH5hCj9JedSIPIuYcmVWIN1D/on4LOdjMaTgRXWDmc96+cJ8T0b1+bWEOFP1rgam9DmTWbHrM3l/3Y84cn5o6edMp5XTU49cZNtuXluc//Ew8KLfdHCXnOwejgzmq804q3BdM5XTX5an6uC9lzPG0o4ZuFfjcfx/538BgBtDHQwCAAA"
            End Get
        End Property
    End Class
End Namespace
