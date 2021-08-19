using System;
using System.Collections.Generic;

namespace IEBroker.net46
{
    // URL의 구성요소 : https://codedragon.tistory.com/5502

    class URLCracker
    {
        private const String DEFAULT_HTTP_SCHEME = "http";
        private const String DEFAULT_HTTP_PORT = "80";

        private const String DEFAULT_HTTPS_SCHEME = "https";
        private const String DEFAULT_HTTPS_PORT = "443";


        public URLCracker(String url, String protocolGeneral, String protocolSecure)
        {
            this.ProtocolGeneral = protocolGeneral;
            this.ProtocolSecure = protocolSecure;
            this.Url = url;
        }

        public URLCracker(String url)
        {
            this.ProtocolGeneral = null;
            this.ProtocolSecure = null;
            this.Url = url;
        }


        private String url;
        private String urlWIthoutProtocol;
        private String urlWIthoutProtocolWIthoutSessionPart;

        private String protocol;
        private String host;
        private String port;
        private String path;
        private String query;
        private String fragment;
        private String sessionPart;

        private Dictionary<String, String> queryMap = new Dictionary<String, String>();
        private Dictionary<String, String> sessionMap = new Dictionary<String, String>();


        public String ProtocolGeneral { get; set; }

        public String ProtocolSecure { get; set; }

        public String Url {
            get
            {
                return this.url;
            }
            set
            {
                this.url = value.Trim();
                StringSpliter(this.url, "://", ref this.protocol, ref this.urlWIthoutProtocol);

                // Extract Session Part
                String temp1 = this.urlWIthoutProtocol.TrimEnd(';'); // Remove tarailing semicolon
                int idx = temp1.LastIndexOf(";");
                if (idx == -1)
                {
                    this.urlWIthoutProtocolWIthoutSessionPart = temp1;
                    this.sessionPart = "";
                }
                else
                {
                    StringSpliter(temp1, idx, ";".Length, ref this.urlWIthoutProtocolWIthoutSessionPart, ref this.sessionPart);
                }
            }
        }

        public String Protocol
        {
            get { return this.protocol; }
        }

        public Boolean IsSecure
        {
            get
            {
                return EqualsIgnoreCase(this.ProtocolSecure, this.Protocol);
            }
        }

        public String Host
        {
            get { return this.host; }
        }

        public String Port
        {
            get { return this.port; }
        }

        public String Path
        {
            get { return this.path; }
        }

        public String Query
        {
            get { return this.query; }
        }

        public String Fragment
        {
            get { return this.fragment; }
        }

        public String SessionPart
        {
            get { return this.sessionPart; }
        }


        public void DoCrack()
        {
            int idx = -1;
            String temp1 = this.urlWIthoutProtocolWIthoutSessionPart;
            String temp2 = null;
            String temp3 = null;

            // Extract Fragment Part
            idx = temp1.LastIndexOf("#");
            if (idx > -1)
            {
                StringSpliter(temp1, idx, "#".Length, ref temp2, ref temp3);
                temp1 = temp2;
                this.fragment = temp3;
            }

            // Extract Query Part
            idx = StringSpliter(temp1, "?", ref temp2, ref temp3);
            if (idx > -1)
            {
                temp1 = temp2;
                this.query = temp3;
            }

            // Extract Path
            idx = StringSpliter(temp1, "/", ref temp2, ref temp3);
            temp1 = temp2;
            this.path = (idx == -1) ? "" : ("/" + temp3);

            // Extract Host & Port
            idx = StringSpliter(temp1, ":", ref temp2, ref temp3);
            this.host = temp2;
            this.port = (idx == -1) ? DEFAULT_HTTP_PORT : temp3;

            // Extract Qeury to Param
            if (!String.IsNullOrEmpty(this.query))
            {
                foreach (String t in this.query.Split('&'))
                {
                    String[] kv = t.Split('=');
                    this.queryMap[kv[0]] = kv[1];
                }
            }

            // Extract Session to Param
            if (!String.IsNullOrEmpty(this.sessionPart))
            {
                foreach (String t in this.sessionPart.Split('&'))
                {
                    String[] kv = t.Split('=');
                    this.sessionMap[kv[0]] = kv[1];
                }
            }
        }



        /// <summary>
        /// Protocol이 변경되고, 된 URL을 반환한다.
        /// </summary>
        /// <returns></returns>
        public String GetProcessedUrl(String newProtocol, Boolean removeSessionPart)
        {
            if (removeSessionPart)
            {
                return newProtocol + "://" + this.urlWIthoutProtocolWIthoutSessionPart;
            }
            else
            {
                return newProtocol + "://" + this.urlWIthoutProtocol;
            }
        }

        public String GetProcessedUrl(String newProtocol)
        {
            return GetProcessedUrl(newProtocol, false);
        }

        public String GetProcessedUrl(Boolean removeSessionPart)
        {
            String newProtocol = this.IsSecure ? DEFAULT_HTTPS_SCHEME : DEFAULT_HTTP_SCHEME;
            return GetProcessedUrl(newProtocol, removeSessionPart);
        }

        public String GetProcessedUrl()
        {
            String newProtocol = this.IsSecure ? DEFAULT_HTTPS_SCHEME : DEFAULT_HTTP_SCHEME;
            return GetProcessedUrl(newProtocol, false);
        }



        private int StringSpliter(String in_str, int idx, int len, ref String out_str_head, ref String out_str_tail)
        {
            if (idx == -1)
            {
                out_str_head = in_str;
                out_str_tail = "";
            }
            else
            {
                out_str_head = in_str.Substring(0, idx);
                out_str_tail = in_str.Substring(idx + len);
            }

            return idx;
        }


        private int StringSpliter(String in_str, String in_token, ref String out_str_head, ref String out_str_tail)
        {
            int idx = in_str.IndexOf(in_token);
            int len = in_token.Length;
            return StringSpliter(in_str, idx, len, ref out_str_head, ref out_str_tail);
        }


        private Boolean EqualsIgnoreCase(String a, String b)
        {
            if (a == null) return false;
            if (b == null) return false;
            return a.ToUpper().Equals(b.ToUpper());
        }
    }
}
