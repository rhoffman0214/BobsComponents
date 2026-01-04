var _____WB$wombat$assign$function_____ = function (name) {
  return (
    (self._wb_wombat &&
      self._wb_wombat.local_init &&
      self._wb_wombat.local_init(name)) ||
    self[name]
  );
};
if (!self.__WB_pmw) {
  self.__WB_pmw = function (obj) {
    this.__WB_source = obj;
    return this;
  };
}
{
  let window = _____WB$wombat$assign$function_____("window");
  let self = _____WB$wombat$assign$function_____("self");
  let document = _____WB$wombat$assign$function_____("document");
  let location = _____WB$wombat$assign$function_____("location");
  let top = _____WB$wombat$assign$function_____("top");
  let parent = _____WB$wombat$assign$function_____("parent");
  let frames = _____WB$wombat$assign$function_____("frames");
  let opener = _____WB$wombat$assign$function_____("opener");

  window.__NUXT__ = (function (a, b, c, d, e, f, g, h, i) {
    return {
      staticAssetsBase: "\u002F_nuxt\u002Fstatic\u002F1706296635",
      layout: "default",
      error: b,
      state: {
        isNavOpen: c,
        isSearchOpen: c,
        isAccountOpen: c,
        rosterWaypointActive: f,
        rosterIndexByAlphabet: b,
        globalData: {},
        websiteLoaded: c,
        isTeamPageActive: c,
        bestSellerProducts: [],
        isShowHeader: f,
        checkout: {
          checkout: {},
          adding: c,
          removing: c,
          loading: c,
          showCart: c,
        },
        fazeone: { user_email: d, login_token: d },
        videoPlayer: {
          playVideoData: b,
          miniPlayer: { status: b, content: b },
          currentlyPlaying: b,
          currentYoutubeVideo: { id: d, name: d, avatar: d },
          currentTwitchClip: b,
          currentStream: b,
          videoAnimation: b,
          videoPending: b,
          currentVideoObject: b,
          windowBeforeUnloadListenerActivated: b,
          videoYouTubeEnded: b,
          videoYouTubeError: b,
          twitchFeedStreamData: b,
          isTwitchFeedStreamActive: c,
          isSizzleVideoPlaying: c,
        },
      },
      serverRendered: f,
      routePath: "\u002F",
      config: {
        api_url:
          "https:\u002F\u002Fweb.archive.org\u002Fweb\u002F20240127030241\u002Fhttps:\u002F\u002Ffazeweb.fazeclan.com",
        domain: "fazeclan.com",
        domain_url:
          "https:\u002F\u002Fweb.archive.org\u002Fweb\u002F20240127030241\u002Fhttps:\u002F\u002Ffazeclan.com",
        backupDomain:
          "https:\u002F\u002Fweb.archive.org\u002Fweb\u002F20240127030241\u002Fhttps:\u002F\u002Fd2054yuzk57130.cloudfront.net",
        shop_url:
          "https:\u002F\u002Fweb.archive.org\u002Fweb\u002F20240127030241\u002Fhttps:\u002F\u002Fshop.fazeclan.com",
        s3_assets_url:
          "https:\u002F\u002Fweb.archive.org\u002Fweb\u002F20240127030241\u002Fhttps:\u002F\u002Fd21is3bk1bus90.cloudfront.net",
        investors_url:
          "https:\u002F\u002Fweb.archive.org\u002Fweb\u002F20240127030241\u002Fhttps:\u002F\u002Finvestor.fazeclan.com",
        sitebackendUrl:
          "https:\u002F\u002Fweb.archive.org\u002Fweb\u002F20240127030241\u002Fhttps:\u002F\u002Fsitebackend.fazeclan.com",
        backendUrl:
          "https:\u002F\u002Fweb.archive.org\u002Fweb\u002F20240127030241\u002Fhttps:\u002F\u002Fbackend.fazeclan.com",
        pressPages: [
          {
            path: "\u002Fpress\u002Frollbit",
            title:
              "FAZE CLAN AND ROLLBIT ANNOUNCE MULTI-MILLION DOLLAR ESPORTS SPONSORSHIP DEAL",
            description:
              "Rollbit to sponsor FaZe Clanâ€™s top-performing Counter-Strike team in one of the largest sponsorship deals in the history of esports",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Frollbit-banner.jpeg",
            date: "January 23, 2024",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fgiphy10bviews",
            title: "FAZE CLAN CELEBRATES REACHING 10 BILLION VIEWERS ON GIPHY",
            description:
              "FAZE CLAN BECOMES ONLY GAMING AND ESPORTS BRAND TO EVER SURPASS THIS MAJOR MILESTONE",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002Fhappy-lets-go.gif",
            date: "November 16, 2023",
            category: a,
          },
          {
            path: "\u002Fpress\u002Ffazeclansteelseries",
            title:
              "FAZE CLAN AND STEELSERIES LAUNCH CO-BRANDED GAMING GEAR AS PART OF MULTI-YEAR PARTNERSHIP",
            description:
              "LIMITED-EDITION COLLECTION FEATURES CUSTOM HEADSET, KEYBOARD, MOUSE & MOUSEPAD",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Ffaze-steelseries.png",
            date: "November 6, 2023",
            category: a,
          },
          {
            path: "\u002Fpress\u002Ffazeclangamesquare",
            title:
              "GameSquare To Acquire One of the Biggest  Names in Gaming, FaZe Clan ",
            description:
              "EXPANDS SCALE AND CAPABILITIES TO CREATE A LEADING CONTENT, MEDIA,  AND ENTERTAINMENT COMPANY FOCUSED ON GAMING, ESPORTS AND YOUTH  CULTURE ",
            date: "October 20, 2023",
            category: a,
          },
          {
            path: "\u002Fpress\u002Ffazearcadelasvegas",
            title:
              "FAZE CLAN WILL BE ACTIVATING IN LAS VEGAS WITH GAMING LOUNGE EXPERIENCE â€˜FAZE ARCADEâ€™ PRESENTED BY XFINITY",
            description:
              "GAMING STATIONS, INTERACTIVE EXPERIENCES, TALENT MEET & GREETS, MERCH GIVEAWAYS &MORE WILL BE AVAILABLE ON OCTOBER 20TH - OCTOBER 21ST AT PALMS CASINO RESORT",
            date: "October 10, 2023",
            category: a,
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Ffaze-arcade-min.png",
          },
          {
            path: "\u002Fpress\u002Ffazeporscherennsport",
            title:
              "FAZE CLAN TO HOST â€œFAZE ARCADEâ€ IN COLLABORATION WITH PORSCHE AT RENNSPORT REUNION 7",
            description:
              "â€œFAZE ARCADEâ€ WILL FEATURE ONE-OF-A-KIND GAMING EXPERIENCES INCLUDING STATE OF THE ART RACING SIMULATORS AND CLASSIC ARCADE GAMES",
            date: "September 25, 2023",
            category: a,
          },
          {
            path: "\u002Fpress\u002Ffazeyourrageannouncement",
            title:
              "FAZE CLAN SIGNS STREAMER & CONTENT CREATOR YourRAGE AS NEWEST OFFICIAL MEMBER",
            description:
              "Meet The New Wave of Streaming Talent: FaZe YourRAGEGamer, Streamer & High-Energy Internet Personality with 5M Followers",
            date: g,
            category: a,
          },
          {
            path: "\u002Fpress\u002F2q23earningsreport",
            title:
              "FaZe Holdings Announces Second Quarter 2023 Financial Results",
            description:
              "LOS ANGELES â€“ August 14, 2023: FaZe Holdings Inc. (Nasdaq: FAZE), the lifestyle and media platform rooted in gaming and youth culture, today announced its financial results for the second quarter, ended June 30, 2023, in a Letter to Shareholders.",
            date: g,
            category: e,
          },
          {
            path: "\u002Fpress\u002Forangechickenpizzarolls",
            title:
              "Totino`s â„¢GetsZesty with New Fan-Favorite Pizza Roll Flavor â€“ Orange Chicken",
            description:
              "In partnership with FaZe Clan, New Orange Chicken Pizza Rollsâ„¢ Snacks Turn Meal-Time Staple into an Innovative Gaming Snack, Available in August",
            date: "August 9, 2023",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fkaysanatlantafaze",
            title:
              "FAZE KAYSAN CONFIRMS MINORITY OWNERSHIP IN FAZE CLANâ€™S CALL OF DUTY TEAM: ATLANTA FAZE",
            description:
              "LOS ANGELES, CA (August 4, 2023) FaZe Clan member FaZe Kaysan officially confirms he has signed on as a minority owner of FaZe Clanâ€™s Call of Duty team, Atlanta FaZe (ATL FaZe) based in Atlanta, Georgia. Kaysanâ€™s new role with ATL FaZe will see him working more closely with the organization and pro players to expand the Call of Duty scene. The news was officially confirmed via ATL FaZeâ€™s Twitter channel HERE.",
            date: "August 4, 2023",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Fkaysan-atl-faze.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fq22023earningsannouncement",
            title:
              "FaZe Holdings Inc. to Announce Second Quarter 2023 Financial Results",
            description:
              "LOS ANGELES â€“ August 3, 2023 â€“ FaZe Holdings Inc. (Nasdaq: FAZE), the lifestyle and media platform rooted in gaming and youth culture, announced that it will release its financial results on Monday, August 14, 2023, for the second quarter ended June 30, 2023. FaZe will issue a press release when its Shareholder Letter has been posted to its Investor Relations website at https:\u002F\u002Finvestor.fazeclan.com.",
            date: "August 3, 2023",
            category: a,
          },
          {
            path: "\u002Fpress\u002Ffazedrazahannouncement",
            title:
              "ZACK â€˜DRAZAHâ€™ JORDAN SIGNS TO FAZE CLANâ€™S CALL OF DUTY TEAM: ATLANTA FAZE",
            description:
              "LOS  ANGELES,  CA  (JULY  20,  2023)  Today,  FaZe  Clanâ€™s  Call  of  Duty  team,  Atlanta  FaZe  (ATL  FaZe)  announced  the signing of 2022 Call of Duty League (CDL) Champion and unrestricted free agent Zack â€˜Drazahâ€™ Jordan as the newest member  of  the  roster.  After  much  speculation  within  the  CDL  community  in  recent  weeks,  the  news  was  made official with an announcement video on FaZe ZooMaaâ€™s Twitch livestream (https:\u002F\u002Fwww.twitch.tv\u002Fzoomaa) - watch the content HERE featuring FaZe Kaysan, FaZe Crowder and FaZe Drazah.",
            date: "July 20, 2023",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002Fdrazah-1.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Ffazebluefilleannouncement",
            title:
              "FAZE CLAN WELCOMES STRANGER THINGS ACTRESS AND TWITCH STREAMER GRACE VAN DIEN",
            description:
              "INTRODUCING FAZE BLUEFILLE; FAZEâ€™S NEWEST GAMER GIRL IS ON A MISSION TO BRING FEMALES TO THE FOREFRONT",
            date: "May 25, 2023",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002F1sTmzU3eISZyXtWlTZXYmnse4G_6fuCNkg-sGlK8oyAdk5wj3WA.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fexecutiveannouncements",
            title: "FAZE HOLDINGS ANNOUNCES LEADERSHIP APPOINTMENTS",
            description:
              "CHRISTOPH PACHLER NAMED CHIEF OPERATING OFFICER AND CHIEF FINANCIAL OFFICER, FAZE HOLDINGS; ERIK ANDERSON NAMED PRESIDENT, FAZE CLAN",
            date: "May 17, 2023",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002FexecutiveAnnouncements-min.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002F1q23earningsannouncement",
            title:
              "FaZe Holdings Inc. Announces First Quarter 2023 Financial Results",
            description:
              "LOS ANGELES â€“ May 15, 2023 â€“ FaZe Holdings Inc. (Nasdaq: FAZE), the lifestyle and media platform rooted in gaming and youth culture, today posted its financial results for the first quarter, ended March 31, 2023 in a Letter to Shareholders. The Letter to Shareholders can be accessed on FaZeâ€™s Investor Relations website, investors.fazeclan.com.",
            date: "May 15, 2023",
            category: e,
          },
          {
            path: "\u002Fpress\u002Ffy22earnings",
            title:
              "FaZe Holdings Inc. Announces Fourth Quarter and Full Year 2022 Financial Results",
            description:
              "LOS ANGELES â€“ March 30, 2023 â€“ FaZe Holdings Inc. (Nasdaq: FAZE), the lifestyle and media platform rooted in gaming and youth culture, today posted its financial results for the fourth quarter and fiscal year ended December 31, 2022 in a Letter to Shareholders. The Letter to Shareholders can be accessed on FaZeâ€™s Investor Relations website.",
            date: "March 30, 2023",
            category: e,
          },
          {
            path: "\u002Fpress\u002Fcsgointelgrandslam2023",
            title:
              "FAZE CLAN WINS INTEL GRAND SLAM IN COUNTER STRIKE: GLOBAL OFFENSIVE - A RECORD-BREAKING ACCOMPLISHMENT AS THE FIRST INTERNATIONAL ROSTER TO REACH THIS MILESTONE",
            description:
              "THE GRAND SLAM COMES AFTER BRINGING HOME EPL SEASON 17 AND OVER $1,000,000 IN PLAYER WINNINGS",
            date: "March 27, 2023",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Ffaze-clan-csgo-final.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fbkaugustinpromotion",
            title:
              "FAZE CLAN PROMOTES MATT â€œBKâ€ AUGUSTIN TO VICE PRESIDENT OF BRAND & CREATIVE STRATEGY",
            description:
              "(Los  Angeles,  CA)  March  23,  2023  â€“  FaZe  Holdings  Inc.  (Nasdaq:  FAZE)  (â€œFaZe  Clanâ€),  the  lifestyle  and media  platform  rooted  in  gaming  and  youth  culture,  today  announced  Matt  â€œBKâ€  Augustin  has  been promoted to Vice President of Brand & Creative Strategy. Augustin, who has been at FaZe Clan for just over two years as Director of Brand & Creative Strategy, will lead a team that will work cross-functionally to optimize FaZe Clan`s creative, brand and media strategy. Augustin will report to President and COO, Zach Katz.",
            date: "March 23, 2023",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Faugustin-headshot.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Ffemalevalorantannouncement",
            title:
              "FAZE CLAN SIGNS ORGANIZATIONâ€™S FIRST ALL FEMALE PROFESSIONAL ESPORTS TEAM",
            description:
              "INTRODUCING FAZEâ€™S NEW FEMALE VALORANT ROSTER:FAZE REFINNEJ, FAZE EMY, FAZE PANINI,  FAZE MADDIESUUN AND FAZE DI^",
            date: "March 7, 2023",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002FfaZe-clan-female-valorant-team.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Ffazepopannouncement",
            title:
              "GHOSTÂ® and FaZe Clanâ„¢ Launch Epic GHOSTÂ® ENERGY and GHOSTÂ® GAMER Flavor: FAZE POPâ„¢",
            description:
              "As part of their record-breaking partnership, the brands launch a product collab thatâ€™s sure to take you back",
            date: "February 22, 2023",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002Ffazepop-1.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fbapeannouncement",
            title:
              "FAZE CLAN AND A BATHING APEÂ® ANNOUNCE LIMITED-EDITION MERCHANDISE COLLABORATION",
            description: "FULL COLLECTION AVAILABLE FEBRUARY 18TH",
            date: "February 13, 2023",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Ffaze-bape-nate.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fnikeannouncement",
            title:
              "FaZe Clan and Nike officially confirm their first product collaboration:the Nike LeBron Nxxt Gen x FaZe Clan.",
            description:
              "Later tonight, Sierra Canyon and Bronny James will officially debut the shoe during the Sierra Canyon vs. Notre Dame game at UCLAâ€™s Pauley Pavilion at 8pm PT. In addition to Bronny and the team repping the shoe in-game, FaZe Clan talent will be sitting courtside to support and celebrate the official reveal and confirmation of the collaboration.",
            date: "January 30, 2023",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002Fnike-1.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fporscheannouncement",
            title: "FAZE CLAN AND PORSCHE ANNOUNCE MULTI-YEAR PARTNERSHIP",
            description:
              "PARTNERSHIP INITIATIVES TO INCLUDE ORIGINAL CONTENT, CONSUMER PRODUCTS, ESPORTS INTEGRATIONS AND WEB3 DIGITAL GOODS",
            date: "January 19, 2023",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Ffaze-porsche-swagg.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fstaffannouncement",
            title: "FAZE CLAN ANNOUNCES EXECUTIVE APPOINTMENTS AND PROMOTIONS",
            description:
              "RANA KAPLAN NAMED GENERAL MANAGER, PLUS ADDITIONAL LEADERSHIP APPOINTMENTS ACROSS TALENT, CREATIVE, CONTENT AND MORE",
            date: "January 9, 2023",
            category: a,
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002Fstaff-new4.png",
          },
          {
            path: "\u002Fpress\u002Fovoannouncement",
            title:
              "OVOÂ®AND FAZE CLAN ANNOUNCE PARTNERSHIP & FIRST OFFICIAL COLLABORATION",
            description:
              "FULL COLLECTION INCLUDES OVOÂ®â€™S FIRST-EVER GAMING CONTROLLER AND MOUSEPAD,PLUS HOODIES, VARSITY JACKET & MORE â€“ AVAILABLE DECEMBER 21ST AT OVOÂ® ONLINE, & ALL OVOÂ® RETAIL LOCATIONS GLOBALLY",
            date: "December 16, 2022",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Ffaze-ovo-promo-new.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fkaysanplentyrelease",
            title:
              "FAZE KAYSAN RELEASES NEW SINGLE â€œPLENTYâ€ FEAT. NARDO WICK, G HERBO, BIG30 AND BABYFACE RAY TODAY",
            description:
              "FULL MUSIC VIDEO FEATURING ALL FIVE ARTISTS DROPS AT 2PM ET TODAY",
            date: "December 2, 2022",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002F1JsOBCQQBfi-cy5Dt9M09ELHB2g42eF1GwMiWS3T9zVUAPE1dbg-new.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Ffazerugrugratsannouncement",
            title:
              "FAZE RUG ANNOUNCES LIMITED-EDITION MERCH COLLABORATION WITH NICKELODEONâ€™S RUGRATS TITLED â€œTHE GALAVERSEâ€",
            description:
              "FULL COLLECTION AVAILABLE ONLINE STARTING DECEMBER 1ST INCLUDING MOUSEPADS, HOODIES, AND MORE",
            date: "November 29, 2022",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Fpress-rug-promo.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fq3earnings22",
            title:
              "FaZe Holdings Inc. Reports Third Quarter 2022 Financial Results",
            description:
              "Revenues up 12% year-over-year driven by sponsorship, content and esports",
            date: "November 14, 2022",
            category: e,
          },
          {
            path: "\u002Fpress\u002Fxfinityannouncement",
            title:
              "XFINITY BECOMES OFFICIAL INTERNET AND MOBILE PARTNER OF FAZE CLAN",
            description:
              "Partnership will Bring an Exclusive College Concert Series to Fans Featuring Offset Along with a New Original Content Series Delivering Game Set-Ups For Underserved Creators And Communities",
            date: "October 20, 2022",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Fxfinityannouncement-promo.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Frespawngamingchair",
            title:
              "RESPAWN x FaZe Clan Unleash Limited Edition Gaming Chairs\n",
            description:
              "Highly Anticipated Chair Collaboration Combines the Best in Ergonomics of Fan-Favorite RESPAWN Chairs with FaZe Clanâ€™s High-Energy, Iconic Design \n",
            date: "October 4, 2022",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Frespawn_chair1.jpeg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fnightonthemoon",
            title:
              "FaZe Clan Celebrates Gaming & Content Creator Communities with Biggest Names with Surprise Performance by Travis Scott: RGB: A NIGHT ON THE MOON",
            description:
              "FOR THE FIRST TIME EVER, THE BIGGEST NAMES IN GAMING AND CONTENT CREATION UNITED FOR AN EPIC â€œNIGHT ON THE MOONâ€ EVENT IN SAN DIEGO DURING TWITCHCON WEEKEND WITH A SURPRISE PERFORMANCE BY TRAVIS SCOTT",
            date: "October 7, 2022",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Fnight-on-the-moon1.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fthesandboxannouncement",
            title:
              "FAZE CLAN ENTERS THE METAVERSE IN PARTNERSHIP WITH THE SANDBOXTO BRING â€œFAZE WORLDâ€ TO LIFE",
            description:
              "The Sandbox Will Hold a LAND Sale Around FaZe World to Be Announced Later This Year",
            date: "September 15, 2022",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002Fsandbox-promo.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fmcdonaldsrenewal",
            title:
              "FAZE CLAN ANNOUNCES PARTNERSHIP RENEWAL WITH McDONALDâ€™S USA",
            description:
              'LOS  ANGELES,  CA  (August  31,  2022)  -  FaZe  Holdings  Inc.  (Nasdaq:  FAZE)  ("FaZe  Clan"),  the  lifestyle  and media platform rooted in gaming and youth culture, announced today the renewal of its partnership with McDonaldâ€™s  USA.  The  partnership,  which  began  last  year,  will  continue  to  focus  on  innovative  ways  to connect  with  the  next  generation  of  gamers  and  will  advance  successful  campaigns  from  the  past  year while launching new initiatives.',
            date: "August 31, 2022",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fchristophpachlerannouncement",
            title: "FAZE HOLDINGS INC. APPOINTS NEW CHIEF FINANCIAL OFFICER",
            description:
              'LOS ANGELES, CA (August 25, 2022) - FaZe Holdings Inc., (Nasdaq: FAZE) ("FaZe Clan"), the lifestyle and media  platform  rooted  in  gaming  and  youth  culture,  today  announced  the  appointment  of  Christoph Pachler as Chief Financial Officer, effective on or before October 3, 2022.',
            date: "August 25, 2022",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fq2earnings22",
            title:
              "FaZe Holdings Inc. Reports Second Quarter 2022 Financial Results",
            description:
              "LOS ANGELES, August 15, 2022 â€“ FaZe Holdings Inc. (Nasdaq: FAZE) (â€œFaZeâ€ or the â€œCompanyâ€), the lifestyle and media platform rooted in gaming and youth culture, today filed its financial results for the second quarter ended June 30, 2022 with the SEC. Summary financial results are included with this press release.",
            date: "August 15, 2022",
            category: e,
          },
          {
            path: "\u002Fpress\u002Fnflandfazedeestroying",
            title:
              "FAZE CLAN AND THE NFL BRING THE â€˜1ON1â€™ SERIES WITHFAZE DEESTROYING TO MULTIPLE CITIES DURING PRESEASON",
            description:
              "LOS ANGELES, CA (July 28, 2022) â€“ FaZe Clan, the lifestyle and media platform rooted in gaming and youth culture, and the National Football League (NFL) today announced Donald De La Haye â€“ also known as FaZe Deestroying â€“ will take the â€˜1ON1â€™ series to cities across the country during NFL preseason.",
            date: "July 28, 2022",
            category: a,
          },
          {
            path: "\u002Fpress\u002Ffazesubsannouncement",
            title:
              "FAZE CLAN LAUNCHES â€œFAZE SUBSâ€EXCLUSIVELY AVAILABLE ON DOORDASH",
            description:
              "FAZE RUG BRINGS THE PARTNERSHIP TO LIFE GIVING FANS A FIRST LOOK AT THE NEW SANDWICHES FEATURING SELF-TITLED SUB â€œTHE RUGFATHERâ€",
            date: "July 26, 2022",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Ffaze-subs-banner-new.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fsellouts",
            title:
              "FaZe Clan, a Leading Gaming, Lifestyle and Media Brand, Completes Business Combination with B. Riley Principal 150 Merger Corp.",
            description:
              "Trading to commence on NASDAQ under the ticker â€œFAZEâ€",
            date: "July 20, 2022",
            category: a,
          },
          {
            path: "\u002Fpress\u002Ffazeronaldoannouncement",
            title:
              "FAZE CLAN WELCOMES POPULAR STREAMER AND PROFESSIONAL GAMER STABLE RONALDO AS NEWEST OFFICIAL MEMBER",
            description:
              "MEET FAZE RONALDO THE 19-YEAR-OLD STREAMER JOINS THE FAZE CLAN ROSTER",
            date: "June 30, 2022",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Ffaze-ronaldo-main-new.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fs4effective",
            title:
              "B. Riley Principal 150 Merger Corp. and FaZe Clan Announce Effectiveness of Form S-4 Registration Statement",
            description:
              "LOS  ANGELES,  CA and NEW  YORK,  NY  --  June  22,  2022 --  B.  Riley  Principal  150  Merger  Corp. (Nasdaq: BRPM) (â€œBRPMâ€), a special purpose acquisition company sponsored by an affiliate of B. Riley Financial, Inc. (Nasdaq: RILY), and FaZe Clan Inc. (â€œFaZe Clanâ€), today announced that the registration statement on Form S-4 (the â€œRegistration Statementâ€) with respect to the proposed business combination between BRPM and FaZe Clan (the â€œBusiness Combinationâ€) was declared effective by the Securities and Exchange Commission (the â€œSECâ€).",
            date: "June 22, 2022",
            category: a,
          },
          {
            path: "\u002Fpress\u002Ftotinosannouncement",
            title:
              "Totinoâ€™sâ„¢ and FaZe Clan Team Up for New Pizza Rolls in one of the most Fan-Requested Flavorsâ€”Buffalo Chicken",
            description:
              "New Buffalo Chicken Pizza Rollsâ„¢ Deliver the Perfect Gaming Snack, Available this Summer",
            date: "June 2, 2022",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Fpress-totinos1.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Flyricallemonadeannouncement",
            title:
              "FAZE CLAN REUNITES WITH LYRICAL LEMONADE TO USHER THE MULTIMEDIA COMPANY INTO THEIR FIRST-EVER IN-GAME COLLABORATION",
            description:
              "FAZE CLAN X LYRICAL LEMONADE COLLABORATION INCLUDES CUSTOM FIFA KITS THAT WILL BE AVAILABLE DIGITALLY INSIDE EA SPORTSâ„¢ FIFA 22 AND AS PHYSICAL APPAREL",
            date: "May 31, 2022",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Ffazeclan-lyrical-lemonade-front-1-new.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Ffazeprozeannouncement",
            title: "Meet FaZe Proze - The FaZe1 Winner!",
            description:
              "FAZE CLAN WELCOMES NEWEST MEMBER & AWARDS THEM A LIFE-CHANGING $1MILLION IN CRYPTO FROM MOONPAY. PLUS $250K SPONSORSHIP FROM GHOST & A BRAND NEW NISSAN GT-R",
            date: "May 26, 2022",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002Fproze1-new.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Ffazexdisneyannouncement",
            title:
              "FAZE CLAN INTRODUCES â€œMICKEY ON THE GRIDâ€A NEW CAPSULE COLLECTION IN COLLABORATION WITH DISNEY",
            description:
              "THE â€˜MICKEY ON THE GRIDâ€™ CAPSULE DROPS WITH LIMITED PRODUCTS ON MAY 28TH AT â€˜THE ARMORY,â€™ FAZE CLANâ€™S POP-SHOP & GAMING LOUNGE IN LOS ANGELES",
            date: "May 23, 2022",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002Fdisney-faze-1.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fkaysanmvprelease",
            title:
              "FAZE KAYSAN RELEASES SECOND SINGLE â€œMVPâ€ FEAT. FIVIO FOREIGN & SHECK WES TODAY",
            description:
              "MUSIC VIDEO FEATURING ALL THREE ARTISTS DROPS TODAY AT 9AM PT",
            date: "May 20, 2022",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Fkaysan-image-1.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fzachkatzannouncement",
            title:
              "FAZE CLAN APPOINTS ZACH KATZ TO NEW ROLE OF PRESIDENT & CHIEF OPERATING OFFICER",
            description:
              "Today, FaZe Clan Inc., the lifestyle and media platform rooted in gaming and youth culture, announced the appointment of Zach Katz as President and Chief Operating Officer, effective immediately.",
            date: "May 19, 2022",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002Ffaze-chief.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fnarutoannouncement",
            title:
              "FAZE CLAN ANNOUNCES MERCHANDISE COLLABORATION WITH ANIME GIANT NARUTO SHIPPUDEN",
            description:
              "COLLABORATION FEATURES AN ARRAY OF BRANDED PRODUCTS INCLUDING FIRST EVER NARUTO SHIPPUDEN 3-D KEYCAPS",
            date: "May 16, 2022",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fdoordashannouncement",
            title: "FAZE CLAN AND DOORDASH ANNOUNCE PARTNERSHIP",
            description:
              "DOORDASH NAMED FAZE CLANâ€™S OFFICIAL ON-DEMAND DELIVERY PLATFORM",
            date: "May 10, 2022",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fduckyannouncement",
            title:
              "FAZE CLAN EXPANDS INTO GAMING PRODUCTS WITH RENOWNED ELECTRONICS BRAND DUCKY",
            description:
              "THE COLLABORATION MARKS FAZE CLANâ€™S FIRST GAMING KEYBOARD RELEASE",
            date: "May 9, 2022",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Fducky-keyboard-new.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fghostannouncement",
            title: "FAZE CLAN AND GHOST ANNOUNCE MULTI-YEAR PARTNERSHIP",
            description:
              "GHOST WILL AWARD BRAND SPONSORSHIP DEAL TO WINNER OF THE FAZE1 GLOBAL RECRUITMENT CHALLENGE",
            date: "May 5, 2022",
            category: a,
          },
          {
            path: "\u002Fpress\u002Ffazeclanandmakeawish",
            title:
              "FAZE CLAN MAKES 20TH WISH COME TRUE IN PARTNERSHIP WITH MAKE-A-WISH",
            description:
              "EARLIER THIS MONTH, FAZE CLAN WELCOMED GAMER FANATIC GABE â€œSHRAYâ€ MOORE FOR A SPECIAL EVENT AT THE NEW FAZE WAREHOUSE MARKING THE ORGANIZATIONâ€™S 20TH GRANTED WISH",
            date: h,
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002Fmake-a-wish-1.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fthearmoryannouncement",
            title:
              "FAZE CLAN ANNOUNCES â€œTHE ARMORYâ€ & NAMES JAY â€œJVYâ€ RICHARDSON CREATIVE DIRECTOR",
            description:
              "GAMING LOUNGE AND RETAIL STORE TO CELEBRATE FAZE CLANâ€™S EXPANSION OF CONSUMER PRODUCTS INTO ELECTRONICS & GAMING GEAR",
            date: h,
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Farmory-content.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fcurrentannouncement",
            title:
              "FaZe Clan, Current announce exclusive, long-term partnership",
            description:
              "The fintech to provide tools, resources for leaders of internet culture to engage fans, starting with content integration and giveaways for â€˜Road to FaZe1â€™",
            date: "April 28, 2022",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fnfldraft2022",
            title:
              "FAZE CLAN & THE NATIONAL FOOTBALL LEAGUE TEAM UP AGAIN WITH MULTIPLE ACTIVATIONS DURING NFL DRAFT WEEKEND IN LAS VEGAS",
            description:
              "FAZE RUG TO ANNOUNCE DRAFT PICK FOR THE LOS ANGELES CHARGERS",
            date: "April 14, 2022",
            category: a,
          },
          {
            path: "\u002Fpress\u002Ffazedeestroyingannouncement",
            title:
              "FAZE CLAN WELCOMES TRAILBLAZING CONTENT CREATOR FAZE DEESTROYING AS NEWEST OFFICIAL MEMBER",
            description:
              "MEET THE DYNAMIC ATHLETE AND CONTENT CREATOR DONALD DE LA HAYE WHO QUIT COLLEGE FOOTBALL TO PURSUE YOUTUBE AND NOW HAS A DIEHARD COMMUNITY OF OVER 10 MILLION FANS",
            date: "April 12, 2022",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Fdeestroying1-new.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Frespawnannouncement",
            title: "RESPAWN NAMED OFFICIAL GAMING CHAIR PARTNER OF FAZE CLAN",
            description:
              "NEW FAZE CLAN X RESPAWN CHAIRS AVAILABLE LATER THIS YEAR",
            date: "April 8, 2022",
            category: a,
          },
          {
            path: "\u002Fpress\u002Funfazedandfaze1thewarehouse",
            title:
              "FAZE CLAN UNVEILS NEW ORIGINAL PROGRAMMING BRINGING NEW FORMATS TO TWITCH",
            description:
              "REVOLUTIONARY TWITCH VARIETY SERIES UNFAZED STARRING RECURRING ENSEMBLE CAST AND SPECIAL GUEST HOSTS GOES LIVE EVERY THURSDAY STARTING JUNE 2ND",
            date: "April 6, 2022",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002Funfazed-3.jpeg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fcyberkongzevent",
            title:
              "CYBERKONGZ & FAZE CLAN CELEBRATE NFT|LA WITH THE â€˜KONGZ ARCADEâ€™ AT THE NEW FAZE WAREHOUSE",
            description:
              "EIGHT COVETED KONGZ NFTS GIVEN AS PRIZES TO RAFFLE WINNERS",
            date: "March 29, 2022",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002Fcyberkongz1.jpeg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fboardofdirectors",
            title:
              "FAZE CLAN ANNOUNCES ITS ANTICIPATED PUBLIC COMPANY BOARD OF DIRECTORS",
            description:
              "Broadly-experienced Board to Oversee FaZe Clanâ€™s Anticipated Growth Across Entertainment, Media and Web3",
            date: "March 22, 2022",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Fboard-of-directors.jpeg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fmanchester-city",
            title:
              "FAZE CLAN AND MANCHESTER CITY TO HOST POP-UP EVENT AT ETIHAD STADIUM",
            description:
              "FAZE CBASS, FAZE NATE HILL, FAZE TEEQO FAZE SWAGG, FAZE MONGRAAL,  FAZE JSMOOTH, FAZE SANTANA, FAZE MEW, FAZE VIRUS, FAZE BIZZLE AND FAZE REPLAYS TO ATTEND MANCHESTER CITY VS. LIVERPOOL GAME",
            date: "March 21, 2022",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fsnoopannouncement",
            title:
              "SNOOP DOGG TO JOIN FAZE CLANâ€™S BOARD OF DIRECTORS AND BECOMES NEWEST TALENT MEMBER",
            description:
              "ENTERTAINMENT ICON & AVID GAMER WILL JOIN BOARD AFTER FAZE CLAN BECOMES A PUBLIC COMPANY LATER THIS YEAR",
            date: "March 7, 2022",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fla-kings-partnership",
            title:
              "FAZE CLAN TEAMS UP WITH LA KINGS FOR SECOND MERCHANDISE COLLABORATION",
            description:
              "LIMITED EDITION FAZE CLAN X LA KINGS LACE UP HOODIES WILL ONLY BE AVAILABLE TO PURCHASE ON MARCH 10TH DURING THE LA KINGS VS SAN JOSE SHARKS GAME",
            date: "February 22, 2022",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Fla-kings-black-kit.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fnflpartnership",
            title:
              "NATIONAL FOOTBALL LEAGUE AND FAZE CLAN TEAM UP TO LAUNCH A SERIES OF ACTIVATIONS AT THE CONVERGENCE OF FOOTBALL & GAMING",
            description:
              "NFL AND FAZE CLAN TO HOST FIRST CO-BRANDED FLAG FOOTBALL GAME AHEAD OF SUPER BOWL LVI IN LOS ANGELES WITH NFL LEGENDS BRETT FAVRE AND MICHAEL VICK AS TEAM QUARTERBACKS",
            date: "February 8, 2022",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002Fnfl.jpeg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Ffazeone-moonpay",
            title:
              "FAZE CLAN ANNOUNCES BIGGEST RECRUITMENT CHALLENGE TO DATE: FAZE1, POWERED BY MOONPAY",
            description:
              "TOP 20 CONTENDERS FROM AROUND THE GLOBE TO MOVE TO LA FOR 24\u002F7 LIVESTREAM SERIES CAPTURING THE FINAL STAGE OF THE COMPETITION",
            date: "January 20, 2022",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Ffazeone-moonpay-banner.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fhalo-faze",
            title:
              "FAZE CLAN UNVEILS STAR STUDDED HALO ROSTER AFTER PROFESSIONALLY ENTERING INTO HALO FOR THE FIRST TIME",
            description:
              "ALCATED, BUBU DUBU, BOUND AND SNIP3DOWN WILL MAKE THEIR DEBUT REPRESENTING FAZE CLAN ON DECEMBER 17 DURING THE HALO CHAMPIONSHIP SERIES 2021: KICKOFF MAJOR",
            date: "December 13, 2021",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002Fhalo-img1-new.png",
            category: a,
          },
          {
            path: "\u002Fmoonpay",
            title:
              "FAZE CLAN ANNOUNCES MOONPAY AS OFFICIAL CRYPTO AND NFT PARTNER IN MULTI-YEAR DEAL",
            description:
              "PIONEERS OF GAMING CULTURE ENTER DIGITAL GOODS INDUSTRY WITH PAYMENT INFRASTRUCTURE PROVIDER ROOTED IN CRYPTO SAFETY AND SECURITY",
            date: "December 7, 2021",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fmcdonalds-spotlight",
            title:
              "FAZE CLAN AND McDONALDâ€™S USA LAUNCH â€œSPOTLIGHTâ€ CAMPAIGN WITH MISSION OF ELEVATING DIVERSE VOICES IN GAMING",
            description:
              "FAZE SWAGG AND FAZE JSMOOTH TO HOST LIVESTREAM EVENTS INTRODUCING THEIR AUDIENCE TO UP & COMING STREAMERS",
            date: "November 30, 2021",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Fmcdonalds2-img2.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Ftakashi-murakami",
            title:
              "FAZE CLAN & TAKASHI MURAKAMI TO RELEASE SECOND COLLABORATION AFTER FIRST COLLECTION SELLS OUT IN LESS THAN AN HOUR, WITH OVER $1 MILLION IN SALES",
            description:
              "ESPORTS JERSEYS & MOUSEPADS FEATURING SIGNATURE FLOWER IN BLACK & WHITE AVAILABLE ON BLACK FRIDAY EXCLUSIVELY ON FAZECLAN.COM AND NTWRK",
            date: "November 19, 2021",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002Fsi-image002.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fdraft-kings",
            title:
              "GAMING AND CONTENT TRAILBLAZER FAZE CLAN TEAMS UP WITH DRAFTKINGS",
            description:
              "DEAL UNITES LEADING SPORTS ENTERTAINMENT AND LIFESTYLE BRANDS ACROSS DIGITAL PLATFORMS",
            date: "November 17, 2021",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002Fdraft_kings2.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fzooma",
            title:
              "ZOOMAA RETURNS TO FAZE CLAN AS A CONTENT CREATOR AFTER RETIREMENT FROM COMPETITIVE CALL OF DUTY",
            description:
              "FAZE CLAN TO BRING THE FLANK INTO ORGANIZATIONâ€™S CONTENT REPERTOIRE",
            date: i,
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Fzooma-image002-new.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fnissan-proam",
            title:
              "FAZE CLAN TEAMS UP WITH NISSAN FOR THE NISSAN FAZE CLAN PRO-AM",
            description:
              "FAZE NATE HILL, FAZE BIZZLE, MCFC THREATS, BENJYFISHY , PLAYERS FROM MANCHESTER CITY & MORE TO COMPETE FOR A TOTAL CASH PRIZE OF $100K ON NOVEMBER 4",
            date: i,
            category: a,
          },
          {
            path: "\u002Fpublic",
            title:
              "FAZE CLAN, A LEADING GAMING, LIFESTYLE AND MEDIA PLATFORM, TO BECOME A PUBLICLY LISTED COMPANY THROUGH MERGER WITH B. RILEY PRINCIPAL 150 MERGER CORP. (NASDAQ: BRPM)",
            description:
              "FAZE CLAN EXPECTED TO LIST ON NASDAQ FOLLOWING BUSINESS COMBINATION WITH B. RILEY PRINCIPAL 150 MERGER CORP.",
            date: "October 25, 2021",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fkaysan-faze",
            title:
              "FAZE CLAN ANNOUNCES NEWEST MEMBER FAZE KAYSAN, THE ORGANIZATIONâ€™S FIRST EVER IN-HOUSE MUSIC ARTIST & GAMER HYBRID",
            description:
              "WATCH FAZE KAYSAN GET KNIGHTED INTO FAZE IN A STAR-STUDDED ANNOUNCEMENT VIDEO FEATURING SOME OF THE BIGGEST PERSONALITIES ON THE INTERNET:",
            date: "September 27, 2021",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Fkaysan-image002.jpeg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fbatman",
            title:
              "FAZE CLAN TAKES ON A NEW LIFE FORM AND ENTERS THE DC UNIVERSE WITH THEIR FIRST-EVER ORIGINAL COMIC BOOK IN CELEBRATION OF BATMAN MONTH",
            description:
              "FAZE CLAN WILL ALSO RELEASE A LIMITED-EDITION MERCHANDISE COLLABORATION WITH DC HIGHLIGHTED BY THE ORIGINAL COMIC BOOK ALONGSIDE ESPORTS JERSEYS, TEES,  HOODIE, PJ SET, MOUSEPAD & MORE ON SEPTEMBER 24TH",
            date: "September 8, 2021",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Fbatman-image002.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fcox",
            title: "FAZE CLAN ANNOUNCES INVESTMENT FROM COX ENTERPRISES",
            description:
              "LOS ANGELES, CA (Aug. 19, 2021) â€“ FaZe Clan, the leading digital lifestyle and media platform rooted in gaming and youth culture, announced today a strategic investment from Cox Enterprises.",
            date: "Aug. 19, 2021",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fmcdonalds",
            title:
              "FAZE CLAN PARTNERS WITH MCDONALDâ€™S USA FOR THEIR FIRST SIGNIFICANT MULTI-CONTENT SERIES IN THE GAMING SPACE",
            description:
              "THIS UNION MARKS GAMINGâ€™S BIGGEST QSR PARTNERSHIP TO DATE",
            date: "August 3, 2021",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002Fmcd-image002.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fmurakami",
            title:
              "FAZE CLAN & TAKASHI MURAKAMI LAUNCH LIMITED-EDITION COLLABORATION",
            description:
              "THE WORLD-RENOWNED CONTEMPORARY ARTIST ENTERS THE GAMING SPACE FOR THE FIRST TIME ALONGSIDE FAZE CLAN",
            date: "June 22, 2021",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002FBQpgPITs-new.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fhfc",
            title: "SETH ROGEN JOINS FAZE CLAN FOR A LIVESTREAM",
            description:
              "SQUAD STREAM WITH FAZE BANKS, FAZE NATE HILL & SPECIAL GUESTS ADIN ROSS, MIKE MAJLIK & MORE TO TAKE PLACE ON JUNE 17TH AT 2PM PT",
            date: "June 16, 2021",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002Fhfc-image002.jpg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fblast",
            title:
              "FAZE CLAN MAKES HISTORY AS THE FIRST GAMING ORGANIZATION TO ILLUMINATE THE COVER OF SPORTS ILLUSTRATED",
            description:
              "COVER SPOTLIGHTS POPULAR GAMERS FAZE NICKMERCS, FAZE TEMPERRR, FAZE SWAGG & FAZE RUG ALONGSIDE ATHLETE-GAMER HYBRID SUPERSTARS FAZE K1 & FAZE BRONNY AND INCLUDES INTERVIEWS WITH FAZE BANKS, LEE TRINK, JIMMY IOVINE & PAUL HAMILTON",
            date: "June 10, 2021",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002FVTH3AXK0-new.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fkalei-faze",
            title: "FAZE CLAN WELCOMES POWERHOUSE GAMER FAZE KALEI",
            description:
              "ONE OF THE BEST FEMALE CALL OF DUTY PLAYERS IN THE SCENE, FAZE KALEI WILL PLAY WARZONE TODAY WITH FAZE RAIN, FAZE TEMPERRR & FAZE ADAPT",
            date: "June 3, 2021",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002Fey7w5K_g.jpeg",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fxbox",
            title:
              "FAZE CLAN ANNOUNCES COLLABORATION WITH MICROSOFT FOR XBOX 360 MERCHANDISE COLLECTION IN CELEBRATION OF â€˜FAZE DAYâ€™",
            description:
              "THE COLLABORATION PAYS HOMAGE TO FAZE CLANâ€™S HUMBLE BEGINNINGS AND CELEBRATES THEO RGANIZATIONâ€™S 11-YEAR ANNIVERSARY",
            date: "May 26, 2021",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002FiDi_ylWw-new.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Ffast-company",
            title:
              "FaZe Clan Named to Fast Companyâ€™s Annual List of the Worldâ€™s Most Innovative Companies for 2021",
            description:
              "Worldâ€™s most prominent esports org among top-ranked in the gaming category",
            date: "March 9, 2021",
            category: a,
          },
          {
            path: "\u002Fpress\u002Futa",
            title:
              "UTA SIGNS GLOBAL GAMING & ENTERTAINMENT POWERHOUSE FAZE CLAN",
            description:
              "BEVERLY HILLS â€“ February 2, 2021 â€“ Leading global talent and entertainment company UTA, today announced that it has signed FaZe Clan, the worldâ€™s most prominent gaming and entertainment organization.",
            date: "February 2, 2021",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fkyler-murray",
            title:
              "FaZe Clan welcomes NFL quarterback ad avid gamer KYLER MURRAY (FAZE K1) as newest member",
            description:
              "FAZE K1 MERCH CAPSULE DROPS THURSDAY, APRIL 29TH  COINCIDING WITH THE 2021 NFL DRAF",
            date: "April 26, 2020",
            thumbnail_url: "\u002Fimages\u002Fpress-kit\u002FT94-3LYc-new.png",
            category: a,
          },
          {
            path: "\u002Fpress\u002Fnuke-squad",
            title: "FAZE CLAN WELCOMES NUKE SQUAD AS OFFICIAL MEMBERS",
            description:
              "POPULAR MEMBER FAZE SWAGG SURPRISED LONG-TIME FRIENDS BY ANNOUNCING DURING LIVESTREAM LAST NIGHT; WATCH THEIR STUNNED REACTIONS BELOW",
            date: "March 24, 2020",
            thumbnail_url:
              "\u002Fimages\u002Fpress-kit\u002Fnuke-squad-image002.jpg",
            category: a,
          },
        ],
        blank_square_svg:
          "data:image\u002Fsvg+xml,%3C%3Fxml version='1.0' encoding='utf-8'%3F%3E%3C!-- Generator: Adobe Illustrator 22.0.1, SVG Export Plug-In . SVG Version: 6.00 Build 0) --%3E%3Csvg version='1.1' id='Ð¨Ð°Ñ€_1' xmlns='https:\u002F\u002Fweb.archive.org\u002Fweb\u002F20240127030241\u002Fhttp:\u002F\u002Fwww.w3.org\u002F2000\u002Fsvg' xmlns:xlink='https:\u002F\u002Fweb.archive.org\u002Fweb\u002F20240127030241\u002Fhttp:\u002F\u002Fwww.w3.org\u002F1999\u002Fxlink' x='0px' y='0px' viewBox='0 0 192.8 192.8' fill='%23fff' style='enable-background:new 0 0 192.8 192.8;' xml:space='preserve'%3E%3Cg id='Layer_2'%3E%3C\u002Fg%3E%3C\u002Fsvg%3E%0A",
        player_sileutte_svg:
          "data:image\u002Fsvg+xml,%3C%3Fxml version='1.0' encoding='utf-8'%3F%3E%3C!-- Generator: Adobe Illustrator 22.0.1, SVG Export Plug-In . SVG Version: 6.00 Build 0) --%3E%3Csvg version='1.1' id='Ð¨Ð°Ñ€_1' xmlns='https:\u002F\u002Fweb.archive.org\u002Fweb\u002F20240127030241\u002Fhttp:\u002F\u002Fwww.w3.org\u002F2000\u002Fsvg' xmlns:xlink='https:\u002F\u002Fweb.archive.org\u002Fweb\u002F20240127030241\u002Fhttp:\u002F\u002Fwww.w3.org\u002F1999\u002Fxlink' x='0px' y='0px' viewBox='0 0 192.8 192.8' style='enable-background:new 0 0 192.8 192.8;' xml:space='preserve'%3E%3Cg id='Layer_2'%3E%3Cg id='Layer_1-2'%3E%3Cpath d='M153.1,156.5c-0.1-4.6-0.3-9.2-0.6-13.8c-0.2-2.1-0.5-4.1-0.9-6.2c-0.3-2-0.9-3.9-1.1-5.8c-0.3-2.4-0.2-4.8-0.5-7.2 c-0.2-1.4,0.1-2.5,1.4-3.1s1.4-1.6,0.9-2.9c-0.6-1.6-1.1-3.2-1.5-4.8c-1.1-5.9-3.1-11.7-4-17.7c-0.7-4.7-2.7-9.1-4.2-13.6 c-1.5-4.7-5-8.5-9.6-10.3c-2.1-0.9-4.2-1.7-6.2-2.7c-3.4-1.7-6.8-3.5-10.1-5.4c-1.1-0.5-2.1-1.3-2.9-2.3c-1.2-1.7-2.2-3.5-3-5.4 c-0.5-1.2-0.7-2.5-0.7-3.9c0.2-1.6,0.4-3.2,0.8-4.8c0.4-2.6,2.3-4.4,3.7-6.8c0.7,3.4,2,5.7,5.9,4.2l0.3,1.7 c1.6-2.1,3.2-4.1,4.6-6.2c0.6-1.1,1.1-2.2,1.4-3.4c1-3.7,1.6-7.4,0.9-11.2c-0.4-1.7-0.9-3.4-1.4-5c-0.2-0.6-0.7-1-0.8-1.6 c-1.5-4.1-4.4-7.3-7.4-10.3c-2.2-2-4.7-3.7-7.3-5.2c-4.5-2.8-9.4-2.4-14.2-0.9c-1.3,0.4-2.5,1.2-3.8,1.7 c-4.7,1.8-8.8,4.8-11.9,8.8c-1.9,2.3-3.9,4.5-4.7,7.6c-0.8,3.2-1.7,6.4-0.9,9.7c0.4,2,0.5,4.2,1.3,6.1s2,3.6,2.2,5.8 c0.1,0,0.2,0,0.3,0c0.1-0.7,0.2-1.3,0.2-2c2.9,1.1,5.4,1.4,6.8-2.2c1.5,2.9,1.7,6.2,0.7,9.3c-0.9,3.1-0.8,3.1-4,3.2 c-0.5,0-0.9,0.2-1.2,0.5c-1.1,2.1-2,4.3-3.1,6.4c-0.7,1.4-1.7,2.6-2.9,3.5c-4.5,2.5-9.2,4.7-13.8,7c-2.7,1.3-5.4,2.6-8,4.1 c-2.6,1.4-4.7,3.7-5.6,6.5c-1.8,5.6-3.7,11.2-5.4,16.9c-1.9,6.6-3.4,13.3-5.1,19.9c-0.6,1.5,0,3.3,1.5,4.1 c0.6,0.3,1.2,0.7,1.8,1.1c0.6,0.5,1,1.2,1.2,1.9c0.4,6.3,0.7,12.6,1,18.9c0,0.4,0.1,0.9,0.2,1.3c1.7,5.7,3.4,11.3,5.1,17 c2.5,8.1,6.2,15.6,10,23.2c0.4,0.8,0.6,1.7,0.6,2.6c0,2.4-0.2,4.8-0.3,7.1c0,0.6-0.2,1.3-0.3,1.9h90.5c0-3.2-0.1-6.4,0.9-9.5 c0.4-1.5,0.7-3.1,0.8-4.6c0.2-2.4,0.2-4.9,0.6-7.3C152.2,166.9,153.2,161.7,153.1,156.5z M122.3,16.5c1.2,1.6,2.1,3.5,2.4,5.5 c0,0.3-0.2,0.6-0.5,0.8c-0.3,0.1-0.6,0-0.9-0.2c-1-0.6-1.5-1.8-1.3-2.9c0-1,0-2-0.3-3L122.3,16.5z M80.4,18c0-2.5,1.4-4.8,3.7-5.9 C84.7,15,83.1,17.7,80.4,18z M63,149.6l-0.6,0.1c-0.8-2.6-1.8-5.2-2.4-7.9c-0.6-2.7,1.3-11.1,2.9-14.1h0.5c0,1.4,0,2.8,0,4.2 c0,1.7,0,3.4,0,5.1c0.1,0.9,0,1.9-0.3,2.8c-0.6,1.3-0.6,2.8-0.1,4.2c0.3,1.3,0.5,2.7,0.5,4C63.4,148.4,63.2,149,63,149.6z M137.8,181.8l-0.5-0.1c-0.5-1.8-1.1-3.6-1.6-5.4s-0.9-3.6-1.4-5.3c-0.4-1.6-0.6-3.3-1.1-4.9c-0.3-1.1-0.9-2.1-1.4-3.1 c-0.5-1.1-0.9-2.2-1.1-3.4c-0.3-2.9-0.1-5.9-0.6-8.8c-0.5-3.6,0-7.5-2.1-10.9c-0.3-0.7-0.3-1.4,0-2.1c1.3-2.1,0.9-4.1,0.4-6.3 c-0.5-2.4-0.6-5-0.9-7.5l0.7-0.3c0.4,0.5,0.7,1.1,1,1.8c0.7,2.4,1.2,4.9,1.8,7.4c0.9,3.9,1.8,7.8,2.5,11.8 c0.3,1.7,0.5,3.4,0.5,5.1c-0.1,6.4,1.4,12.5,3,18.6c0.8,2.9,1.5,5.9,2,8.8C139.3,178.9,138.9,180.5,137.8,181.8z'\u002F%3E%3C\u002Fg%3E%3C\u002Fg%3E%3C\u002Fsvg%3E",
        player_sileutte_white_svg:
          "data:image\u002Fsvg+xml,%3C%3Fxml version='1.0' encoding='utf-8'%3F%3E%3C!-- Generator: Adobe Illustrator 22.0.1, SVG Export Plug-In . SVG Version: 6.00 Build 0) --%3E%3Csvg version='1.1' id='Ð¨Ð°Ñ€_1' xmlns='https:\u002F\u002Fweb.archive.org\u002Fweb\u002F20240127030241\u002Fhttp:\u002F\u002Fwww.w3.org\u002F2000\u002Fsvg' xmlns:xlink='https:\u002F\u002Fweb.archive.org\u002Fweb\u002F20240127030241\u002Fhttp:\u002F\u002Fwww.w3.org\u002F1999\u002Fxlink' x='0px' y='0px' viewBox='0 0 192.8 192.8' fill='%23fff' style='enable-background:new 0 0 192.8 192.8;' xml:space='preserve'%3E%3Cg id='Layer_2'%3E%3Cg id='Layer_1-2'%3E%3Cpath fill='%23fff' d='M153.1,156.5c-0.1-4.6-0.3-9.2-0.6-13.8c-0.2-2.1-0.5-4.1-0.9-6.2c-0.3-2-0.9-3.9-1.1-5.8c-0.3-2.4-0.2-4.8-0.5-7.2 c-0.2-1.4,0.1-2.5,1.4-3.1s1.4-1.6,0.9-2.9c-0.6-1.6-1.1-3.2-1.5-4.8c-1.1-5.9-3.1-11.7-4-17.7c-0.7-4.7-2.7-9.1-4.2-13.6 c-1.5-4.7-5-8.5-9.6-10.3c-2.1-0.9-4.2-1.7-6.2-2.7c-3.4-1.7-6.8-3.5-10.1-5.4c-1.1-0.5-2.1-1.3-2.9-2.3c-1.2-1.7-2.2-3.5-3-5.4 c-0.5-1.2-0.7-2.5-0.7-3.9c0.2-1.6,0.4-3.2,0.8-4.8c0.4-2.6,2.3-4.4,3.7-6.8c0.7,3.4,2,5.7,5.9,4.2l0.3,1.7 c1.6-2.1,3.2-4.1,4.6-6.2c0.6-1.1,1.1-2.2,1.4-3.4c1-3.7,1.6-7.4,0.9-11.2c-0.4-1.7-0.9-3.4-1.4-5c-0.2-0.6-0.7-1-0.8-1.6 c-1.5-4.1-4.4-7.3-7.4-10.3c-2.2-2-4.7-3.7-7.3-5.2c-4.5-2.8-9.4-2.4-14.2-0.9c-1.3,0.4-2.5,1.2-3.8,1.7 c-4.7,1.8-8.8,4.8-11.9,8.8c-1.9,2.3-3.9,4.5-4.7,7.6c-0.8,3.2-1.7,6.4-0.9,9.7c0.4,2,0.5,4.2,1.3,6.1s2,3.6,2.2,5.8 c0.1,0,0.2,0,0.3,0c0.1-0.7,0.2-1.3,0.2-2c2.9,1.1,5.4,1.4,6.8-2.2c1.5,2.9,1.7,6.2,0.7,9.3c-0.9,3.1-0.8,3.1-4,3.2 c-0.5,0-0.9,0.2-1.2,0.5c-1.1,2.1-2,4.3-3.1,6.4c-0.7,1.4-1.7,2.6-2.9,3.5c-4.5,2.5-9.2,4.7-13.8,7c-2.7,1.3-5.4,2.6-8,4.1 c-2.6,1.4-4.7,3.7-5.6,6.5c-1.8,5.6-3.7,11.2-5.4,16.9c-1.9,6.6-3.4,13.3-5.1,19.9c-0.6,1.5,0,3.3,1.5,4.1 c0.6,0.3,1.2,0.7,1.8,1.1c0.6,0.5,1,1.2,1.2,1.9c0.4,6.3,0.7,12.6,1,18.9c0,0.4,0.1,0.9,0.2,1.3c1.7,5.7,3.4,11.3,5.1,17 c2.5,8.1,6.2,15.6,10,23.2c0.4,0.8,0.6,1.7,0.6,2.6c0,2.4-0.2,4.8-0.3,7.1c0,0.6-0.2,1.3-0.3,1.9h90.5c0-3.2-0.1-6.4,0.9-9.5 c0.4-1.5,0.7-3.1,0.8-4.6c0.2-2.4,0.2-4.9,0.6-7.3C152.2,166.9,153.2,161.7,153.1,156.5z M122.3,16.5c1.2,1.6,2.1,3.5,2.4,5.5 c0,0.3-0.2,0.6-0.5,0.8c-0.3,0.1-0.6,0-0.9-0.2c-1-0.6-1.5-1.8-1.3-2.9c0-1,0-2-0.3-3L122.3,16.5z M80.4,18c0-2.5,1.4-4.8,3.7-5.9 C84.7,15,83.1,17.7,80.4,18z M63,149.6l-0.6,0.1c-0.8-2.6-1.8-5.2-2.4-7.9c-0.6-2.7,1.3-11.1,2.9-14.1h0.5c0,1.4,0,2.8,0,4.2 c0,1.7,0,3.4,0,5.1c0.1,0.9,0,1.9-0.3,2.8c-0.6,1.3-0.6,2.8-0.1,4.2c0.3,1.3,0.5,2.7,0.5,4C63.4,148.4,63.2,149,63,149.6z M137.8,181.8l-0.5-0.1c-0.5-1.8-1.1-3.6-1.6-5.4s-0.9-3.6-1.4-5.3c-0.4-1.6-0.6-3.3-1.1-4.9c-0.3-1.1-0.9-2.1-1.4-3.1 c-0.5-1.1-0.9-2.2-1.1-3.4c-0.3-2.9-0.1-5.9-0.6-8.8c-0.5-3.6,0-7.5-2.1-10.9c-0.3-0.7-0.3-1.4,0-2.1c1.3-2.1,0.9-4.1,0.4-6.3 c-0.5-2.4-0.6-5-0.9-7.5l0.7-0.3c0.4,0.5,0.7,1.1,1,1.8c0.7,2.4,1.2,4.9,1.8,7.4c0.9,3.9,1.8,7.8,2.5,11.8 c0.3,1.7,0.5,3.4,0.5,5.1c-0.1,6.4,1.4,12.5,3,18.6c0.8,2.9,1.5,5.9,2,8.8C139.3,178.9,138.9,180.5,137.8,181.8z'\u002F%3E%3C\u002Fg%3E%3C\u002Fg%3E%3C\u002Fsvg%3E",
      },
    };
  })(
    "announcements",
    null,
    false,
    "",
    "reports",
    true,
    "August 14, 2023",
    "May 2, 2022",
    "November 4, 2021"
  );
}
/*
     FILE ARCHIVED ON 03:02:41 Jan 27, 2024 AND RETRIEVED FROM THE
     INTERNET ARCHIVE ON 16:26:58 May 11, 2024.
     JAVASCRIPT APPENDED BY WAYBACK MACHINE, COPYRIGHT INTERNET ARCHIVE.

     ALL OTHER CONTENT MAY ALSO BE PROTECTED BY COPYRIGHT (17 U.S.C.
     SECTION 108(a)(3)).
*/
/*
playback timings (ms):
  captures_list: 0.65
  exclusion.robots: 0.092
  exclusion.robots.policy: 0.077
  esindex: 0.01
  cdx.remote: 6.464
  LoadShardBlock: 77.48 (3)
  PetaboxLoader3.datanode: 201.665 (5)
  PetaboxLoader3.resolve: 448.026 (2)
  load_resource: 584.595
  loaddict: 54.206
*/
