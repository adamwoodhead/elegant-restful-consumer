using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataConnection.Models
{
    public abstract class Authenticatable<T> : SoftDeleteDataPacket<T>
    {
        public AuthenticationPacket Authentication { get; set; }

        public abstract string AuthenticateRoute { get; }

        public abstract string RefreshRoute { get; }

        public abstract string LogoutRoute { get; }

        public abstract string Password { get; set; }

        public async Task<bool> AuthenticateAsync(string password, CancellationToken cancellationToken = default)
        {
            string oldPassword = Password;
            Password = password;

            try
            {
                string url = $"{DataConnection.BaseURL}" + AuthenticateRoute;

                RestRequest request = new RestRequest(url, Method.POST, DataFormat.Json);

                request.AddJsonBody(this);

                IRestResponse<AuthenticationPacket> restResponse = await DataConnection.RequestAsync<AuthenticationPacket>(request, cancellationToken);

                Authentication = restResponse.Data;

                Password = oldPassword;

                return true;
            }
            catch (Exception ex)
            {
                // return false;
                Password = oldPassword;
                throw ex;
            }
        }
    }
}
