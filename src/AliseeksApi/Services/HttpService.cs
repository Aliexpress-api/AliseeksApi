using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace AliseeksApi.Services
{
    public class HttpService : IHttpService
    {
        public async Task<HttpResponseMessage> Get(string endpoint, Action<HttpClient> configuration = null)
        {
            var response = new HttpResponseMessage();

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, x502, chain, policy) =>
            {
                return true;
            };

            using (HttpClient client = new HttpClient(handler))
            {
                try
                {
                    if(configuration != null)
                        configuration(client);

			var message = new HttpRequestMessage(HttpMethod.Get, endpoint);
			message.Headers.Add("cookie", "ali_apache_id=10.182.248.35.1494606211812.155335.5; _uab_collina=149460621617782781411267; ali_beacon_id=10.182.248.35.1494606211812.155335.5; cna=h8+cEb5L4VYCAUgVxEDp1VLz; aep_common_f=wC5GWN9l7VIsqH8oduIA/LbMqlUewu1ZDz25KbIy/7F6xyYo0UsOng==; u_info=p33/C0zx3x3Wb9GZsGVL5rWpUEUD89Zr8/v9govYMVQ=; xman_us_t=x_lid=us1024010232&sign=y&x_user=zcx9bLyzKeqFk9X5+d4h2Hsz2+uD7iSj3e84uLVM8lc=&ctoken=ex0rn5rlgn52&need_popup=y&l_source=aliexpress; xman_f=dvj2JBk0mbOoEa4eFE6QGDiNl/ZDeE3AaSbDWlFae/TNwKUvdUrSDkjWmGpIPr+IvaSHPt/xGmXzCX2GuKK0jMjJ/edqqt7NThQc+84wPkX302cYLHvMncfKRpKEwVm5GHMlbn+RdftxDPDo2q4D2IbaPnvlSY0J9zNyCNsw8k5j/YDV0NJnIBgoDT8Pb9kHq3upyHHrenOgysmUoC3JPi+MNtjzN92SmESJKqomwMhQZjYXYXLq4Q12jG0Q5MrkdP9/NgtzobFQrG82Cavj6xJVDjR/HxsGSfOau9sKf5tTOUPaFF8FGM1BRM1ap+fXduej23GeqwgMXsQZtJhjI6THfZ+VsVYtnitA/jV3BkV73bK7pFd7Lu2wer4+KF2eBzl00X0+p6E=; JSESSIONID=0EB0232A4DD813DF3725A8D07897444B; _mle_tmp0=eNrz4A12DQ729PeL9%2FV3cfUx8KvOTLFSMnB1MjAyNnI0cXGxMDR2cTM2NzJ1tHAxMLewNDcxMXFS0kkusTI0sTQ1NTWyMDEzMDDWSUxGE8itsDKojQIAlDAXqw%3D%3D; __utma=3375712.1101585822.1494606219.1495552863.1495552863.1; __utmc=3375712; __utmz=3375712.1495552863.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); aep_history=keywords%5E%0Akeywords%09%0A%0Aproduct_selloffer%5E%0Aproduct_selloffer%0932418503502%0932754230887%0932784833821%0932396152911%0932373802890; aep_usuc_t=aep_mtp=mobi; _umdata=F00747DA5AABF0DEBE5E3DE238BC9FBB1EACA5D7164F5C015124EF4C221061B464E69493A354FC92CD43AD3E795C914C873E37EB32ACEB57C4115806FA9E8AF3; xman_us_f=x_l=0&x_locale=en_US&no_popup_today=n&x_user=US|Alex|Bello|ifm|143472817&zero_order=n&last_popup_time=1494606224832; intl_locale=en_US; aep_usuc_f=region=US&site=glo&b_locale=en_US&aep_cet=mobile&isb=y&isfm=y&x_alimid=143472817&c_tp=USD; intl_common_forever=UP6CI/CPti/yNzmBHidBMQRFTQivF7gTxYiLuCDdpxRhTav6+S/Uzw==; _ga=GA1.2.1101585822.1494606219; _gid=GA1.2.511208879.1495556675; ali_apache_track=mt=1|ms=|mid=us1024010232; ali_apache_tracktmp=W_signed=Y; l=AsPDMj3n-ubpenax9NLW9uuI041t2Fd6; isg=Au_vsvxa6kKvdO5PECrsewmlfgVxp0O2oAdWqAF8a95lUA9SCWTTBu3C_PcU; acs_usuc_t=x_csrf=p4dr2mydj6wd&acs_rt=20574efc4dab4fc09672a434d0f2121d; xman_t=Gd2D+/BIKMn+Sdk/KAvw7DAwsNEWXXQGTziABQIGCTvtmZm/ugwdF9j37XxD+JOAanCNjUCqeEnEhkLq0DQmjgLWGsxRL+7jK72aFMOo9+wS6f7E5A2FuBqpFaKwJwxFd0Uoc9o/qk2jS+ZxI6+y0YYONfIZm+ucg+jvFLrkZ3PDPR7NZBPJL0/26/lLSc234eNfm19308i3mgCpkSW7Ui3a7xRKNp/WPBzgE+yIYAYZfqNppFfax2FeKukApTl4Q5KT12Xf5rPCXaOk/Bme5yrs3EPWI5CH1F7OkpGD2+fYPZ0FUqUaDU41/PYzly+5I+cJWJfXWfjLa8ZrN9S3BDhRqcfb/bmhnw3UcIDptfL1DZ/9OtwyBnBpgFXBPCy6KZS6/fOcSeig0ViFbbOFzl9k5Mey4u9QUvdNwud7972gD78Mz8p2FwzbBjfE9mvTM1vXKcbmEMjHI+Zzknte+ArBk/J+6jnm0SeaZYYqg0IQgP/GaHL2dEE0IwpQP5k1OJlXDHjb0NzCiZzm90hMCwRTl5qjtyUOjycYMClirldVrQrjP01i4/NcdW3Ug6Bu89/gx4n+NyKr9YVvLeLI8effnOrMDcUwhUDcBKGWaj9xY9nozWAtdg==");

                    response = await client.SendAsync(message);
                }
                catch(Exception e)
                {

                }
            }

            return response;
        }

        public async Task<HttpResponseMessage> Post(string endpoint, HttpContent content, Action<HttpClient> configuration = null)
        {
            var response = new HttpResponseMessage();

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, x502, chain, policy) =>
            {
                return true;
            };

            using (HttpClient client = new HttpClient(handler))
            {
                try
                {
                    if (configuration != null)
                        configuration(client);

                    response = await client.PostAsync(endpoint, content);
                }
                catch (Exception e)
                {

                }
            }

            return response;
        }

        public async Task<HttpResponseMessage> Put(string endpoint, HttpContent content, Action<HttpClient> configuration = null)
        {
            var response = new HttpResponseMessage();

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, x502, chain, policy) =>
            {
                return true;
            };

            using (HttpClient client = new HttpClient(handler))
            {
                try
                {
                    if (configuration != null)
                        configuration(client);

                    response = await client.PutAsync(endpoint, content);
                }
                catch (Exception e)
                {

                }
            }

            return response;
        }
    }
}
