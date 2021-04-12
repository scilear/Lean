using System.Collections.Generic;

namespace QuantConnect
{
    /// <summary>
    /// Basic Template Library Class
    /// Library classes are snippets of code you can reuse between projects. They are added to projects on compile. This can be useful for reusing
    /// indicators, math components, risk modules etc. If you use a custom namespace make sure you add the correct using statement to the
    /// algorithm-user.
    /// </summary>
    public static class Universes
    {
    	
    	public static HashSet<string> ETFs = new HashSet<string>{
    		"VDE", "USO", "XES", "XOP", "UNG", "ICLN", "ERX",
                "UCO", "AMJ", "BNO", "AMLP", "UGAZ", "TAN",
			"ERY", "SCO", "DGAZ",
			"GLD", "IAU", "SLV", "GDX", "AGQ", "PPLT", "NUGT", "USLV", "UGLD", "JNUG",
			"DUST", "JDST",
			"QQQ", "IGV", "QTEC", "FDN", "FXL", "TECL", "SOXL", "SKYY", "KWEB",
			"TECS", "SOXS",
			"IEF", "SHY", "TLT", "IEI", "TLH", "BIL", "SPTL",
			                "TMF", "SCHO", "SCHR", "SPTS", "GOVT",
			"SHV", "TBT", "TBF", "TMV",
			"TVIX", "VIXY", "SPLV", "UVXY", "EEMV", "EFAV", "USMV",
			"SVXY",
			"XLB", "XLE", "XLF", "XLI", "XLK", "XLP", "XLU", "XLV", "XLY"
    	};
    	public static HashSet<string> SECTORS = new HashSet<string>{"XLE", "XLF", "XLI", "XLU", "XLK", "XLV", "XLY", "XLP", "XLB", "XLRE", "XLC", };
    	public static HashSet<string> SP500 = new HashSet<string>{
    		"ZTS","ZION","ZBH","ZBRA","YUM","XYL","XLNX","XRX","XEL","WYNN","WLTW","WMB","WHR","WY","WRK","WU","WDC","WST","WELL","WFC","WEC","WAT","WM","DIS","WBA","WMT","WAB","WRB","VMC","VNO","VNT","V","VTRS","VIAC","VFC","VRTX","VZ","VRSK","VRSN","VTR","VAR","VLO","UNM","UHS","URI","UPS","UNH","UAL","UNP","UA","UAA","USB","ULTA","UDR","TSN","TYL","TWTR","TFC","TRV","TDG","TT","TSCO","TJX","TIF","TMO","TXT","TSLA","TTWO","TER","TFX","TDY","FTI","TEL","TGT","TPR","TTWO","TROW","TMUS","SYY","SNPS","SYF","SIVB","SYK","STE","STT","SBUX","SWK","LUV","SO","SNA","SLG","SWKS","SPG","SHW","NOW","SRE","SEE","STX","SLB","SBAC","CRM","SPGI","RCL","ROST","ROP","ROL","ROK","RHI","RMD","RSG","RF","REGN","REG","O","RTX","RJF","RL","DGX","QCOM","PWR","QRVO","PVH","PHM","PSA","PEG","PRU","PLD","PGR","PG","PFG","PPL","PPG","POOL","PNC","PXD","PNW","PSX","PM","PFE","PRGO","PKI","PEP","PBCT","PNR","PYPL","PAYC","PAYX","PH","PKG","PCAR","OTIS","ORCL","OKE","OMC","ODFL","OXY","ORLY","NVR","NVDA","NUE","NRG","NCLH","NLOK","NOC","NTRS","NSC","NI","NKE","NLSN","NEE","NWS","NWSA","NEM","NWL","NFLX","NTAP","NOV","NDAQ","MSCI","MSI","MOS","MS","MCO","MNST","MDLZ","TAP","MHK","MAA","MSFT","MU","MCHP","MGM","MTD","MET","MRK","MDT","MCK","MCD","MXIM","MKC","MA","MAS","MLM","MMC","MAR","MKTX","MPC","MRO","MTB","LYB","LUMN","LOW","L","LMT","LKQ","LYV","LIN","LNC","LLY","LEN","LDOS","LEG","LVS","LW","LRCX","LH","LHX","LB","KR","KHC","KLAC","KMI","KIM","KMB","KEYS","KEY","K","KSU","JNPR","JPM","JCI","JNJ","SJM","JBHT","J","JKHY","IRM","IQV","IPGP","IVZ","ISRG","INTU","IFF","IPG","IP","IBM","ICE","INTC","IR","INCY","ILMN","ITW","INFO","IDXX","IEX","HII","HBAN","HUM","HPQ","HWM","HST","HRL","HON","HD","HOLX","HFC","HLT","HPE","HES","HSY","HSIC","PEAK","HCA","HAS","HIG","HBI","HAL","GWW","GS","GPN","GL","GILD","GPC","GM","GIS","GE","GD","IT","GRMN","GPS","FCX","BEN","FOX","FOXA","FBHS","FTV","FTNT","F","FMC","FLS","FLIR","FLT","FISV","FRC","FE","FITB","FIS","FDX","FRT","FAST","FB","FFIV","XOM","EXR","EXPD","EXPE","EXC","RE","ES","EVRG","ETSY","EL","ESS","EQR","EQIX","EFX","EOG","ETR","EMR","EA","EW","EIX","ECL","EBAY","ETN","EMN","DXC","DD","DRE","DUK","DTE","DOW","DOV","DPZ","D","DLTR","DG","DISH","DISCK","DISCA","DFS","DLR","FANG","DXCM","DVN","XRAY","DAL","DE","DVA","DRI","DHR","DHI","CVS","CMI","CSX","CCI","COST","CTVA","GLW","CPRT","COO","STZ","ED","COP","CXO","CAG","CMA","CMCSA","CL","CTSH","KO","CMS","CME","CLX","CTXS","CFG","C","CSCO","CTAS","CINF","CI","CHD","CB","CMG","CVX","CHTR","SCHW","CF","CERN","CNP","CNC","CE","CDW","CBRE","CBOE","CAT","CTLT","CARR","CCL","KMX","CAH","COF","CPB","CDNS","COG","CHRW","BF.B","BR","AVGO","BMY","BSX","BXP","BWA","BKNG","BA","BLK","BIIB","BIO","BBY","BRK.B","BDX","BAX","BK","BAC","BLL","BKR","AVY","AVB","AZO","ADP","ADSK","ATO","T","AIZ","AJG","ANET","ADM","APTV","AMAT","AAPL","AIV","APA","AOS","AON","ANTM","ANSS","ADI","APH","AMGN","AME","ABC","AMP","AWK","AMT","AIG","AXP","AEP","AAL","AEE","AMCR","AMZN","MO","GOOG","GOOGL","ALL","LNT","ALLE","ALGN","ALXN","ARE","ALB","ALK","AKAM","APD","A","AFL","AES","AAP","AMD","ADBE","ATVI","ACN","ABMD","ABBV","ABT","MMM",
    	};
    	
    	
    	public static HashSet<string> KINGS = new HashSet<string>
    	{"ABM","AWR","CBSH","CINF","CL","CWT","DOV","EMR","FMCB","FRT","GPC","HRL","JNJ","KO","LANC","LOW","MMM","NDSN","NWN","PG","PH","SCL","SJW","SWK","TR","MO","FUL","SYY","UVV","NFG",};	
    	
    	
    	public static HashSet<string> NDX = new HashSet<string>{
    	"ZM","XLNX","XEL","WDAY","WBA","VRTX","VRSK","VRSN","ULTA","TCOM","TXN","TSLA","TTWO","TMUS","SNPS","SBUX","SPLK","SWKS","SIRI","SGEN","ROST","REGN","QCOM","PDD","PEP","PYPL","PAYX","PCAR","ORLY","NXPI","NVDA","NFLX","NTES","MNST","MDLZ","MRNA","MSFT","MU","MCHP","MELI","MXIM","MAR","LULU","LBTYK","LBTYA","LRCX","KHC","KLAC","KDP","JD","ISRG","INTU","INTC","INCY","ILMN","IDXX","GILD","FOX","FOXA","FISV","FAST","FB","EXPE","EXC","EA","EBAY","DLTR","DOCU","DXCM","CSX","COST","CPRT","CMCSA","CTSH","CTXS","CSCO","CTAS","CHKP","CHTR","CERN","CDW","CDNS","AVGO","BKNG","BMRN","BIIB","BIDU","ADP","ADSK","ASML","AMAT","AAPL","ANSS","ADI","AMGN","AMZN","GOOG","GOOGL","ALGN","ALXN","AMD","ADBE","ATVI",
    		
    		
    	};
        public static HashSet<string> NDX99 = new HashSet<string>{"AAL",
"AAPL",
"ADBE",
"ADI",
"ADP",
"ADSK",
"ALGN",
"ALXN",
"AMAT",
"AMGN",
"AMZN",
"ASML",
"ATVI",
"AVGO",
"BIDU",
"BIIB",
"BKNG",
"BMRN",
"CA",
"CDNS",
"CELG",
"CERN",
"CHKP",
"CHTR",
"CMCSA",
"COST",
"CSCO",
"CSX",
"CTAS",
"CTRP",
"CTSH",
"CTXS",
"DISH",
"DLTR",
"EA",
"EBAY",
"ESRX",
"EXPE",
"FAST",
"FB",
"FISV",
"FOX",
"GILD",
"GOOGL",
"HAS",
"HOLX",
"HSIC",
"IDXX",
"ILMN",
"INCY",
"INTC",
"INTU",
"ISRG",
"JBHT",
"JD",
"KHC",
"KLAC",
"LBTYA",
"LBTYK",
"LRCX",
"MAR",
"MCHP",
"MDLZ",
"MELI",
"MNST",
"MSFT",
"MU",
"MXIM",
"MYL",
"NFLX",
"NTES",
"NVDA",
"ORLY",
"PAYX",
"PCAR",
"PYPL",
"QCOM",
"QRTEA",
"REGN",
"ROST",
"SBUX",
"SHPG",
"SIRI",
"SNPS",
"STX",
"SWKS",
"SYMC",
"TMUS",
"TSLA",
"TTWO",
"TXN",
"ULTA",
"VOD",
"VRSK",
"VRTX",
"WBA",
"WDAY",
"WDC",
"WYNN",
"XLNX",
"XRAY"};
        
    }
}