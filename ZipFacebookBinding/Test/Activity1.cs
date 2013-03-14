using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Com.Facebook;
using Android.Util;

namespace Test
{
    [Activity(Label = "LoginSession", MainLauncher = true, Icon = "@drawable/icon")]
    public class LoginUsingActivityActivity : Activity, View.IOnClickListener
    {
        private string URL_PREFIX_FRIENDS = "https://graph.facebook.com/me/friends?access_token=";

        private TextView textInstructionsOrLink;
        private Button buttonLoginLogout,
            getMeButton;
        private Session.IStatusCallback statusCallback;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);
            buttonLoginLogout = (Button)FindViewById(Resource.Id.buttonLoginLogout);
            getMeButton = FindViewById<Button>(Resource.Id.getmeButton);
            textInstructionsOrLink = (TextView)FindViewById(Resource.Id.instructionsOrLink);

            buttonLoginLogout.SetOnClickListener(this);
            getMeButton.SetOnClickListener(this);

            Settings.AddLoggingBehavior(LoggingBehavior.IncludeAccessTokens);

            Session session = Session.ActiveSession;
            if (session == null)
            {
                if (savedInstanceState != null)
                {
                    session = Session.RestoreSession(this, null, statusCallback, savedInstanceState);
                }
                if (session == null)
                {
                    session = new Session(this);
                }
                Session.ActiveSession = session;
                if (session.State.Equals(SessionState.CreatedTokenLoaded))
                {
                    session.OpenForRead(new Session.OpenRequest(this).SetCallback(statusCallback));
                }
            }
            statusCallback = new SessionStatusCallback(this);
            updateView();
        }

        protected override void OnStart()
        {
            base.OnStart();
            Session.ActiveSession.AddCallback(statusCallback);
        }

        protected override void OnStop()
        {
            base.OnStop();
            Session.ActiveSession.RemoveCallback(statusCallback);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            Session.ActiveSession.OnActivityResult(this, requestCode, (int)resultCode, data);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            Session session = Session.ActiveSession;
            Session.SaveSession(session, outState);
        }

        private void updateView()
        {
            Session session = Session.ActiveSession;
            if (session.IsOpened)
            {
                textInstructionsOrLink.Text = URL_PREFIX_FRIENDS + session.AccessToken;
                buttonLoginLogout.SetText(Resource.String.logout);
            }
            else
            {
                textInstructionsOrLink.SetText(Resource.String.instructions);
                buttonLoginLogout.SetText(Resource.String.login);
            }
        }

        public void OnClick(View v)
        {
            if (v.Id == Resource.Id.getmeButton)
            {
                var request = new Request(Session.ActiveSession, "/me", null, HttpMethod.Get, new RequestCallback());
                var async = request.ExecuteAsync();
            }
            else
            {
                if (((Button)v).Text == Resources.GetString(Resource.String.logout))
                {
                    onClickLogout();
                }
                else
                {
                    onClickLogin();
                }
            }
        }

        private void onClickLogin()
        {
            Session session = Session.ActiveSession;
            if (!session.IsOpened && !session.IsClosed)
            {
                session.OpenForRead(new Session.OpenRequest(this).SetCallback(statusCallback));
            }
            else
            {
                Session.OpenActiveSession(this, true, statusCallback);
            }
        }

        private void onClickLogout()
        {
            Session session = Session.ActiveSession;
            if (!session.IsClosed)
            {
                session.CloseAndClearTokenInformation();
            }
        }

        private class SessionStatusCallback : Java.Lang.Object, Session.IStatusCallback
        {
            LoginUsingActivityActivity _parent;
            public SessionStatusCallback(LoginUsingActivityActivity parent)
            {
                _parent = parent;
            }

            public void Call(Session p0, SessionState p1, Java.Lang.Exception p2)
            {
                _parent.updateView();
            }
        }

        private class RequestCallback : Java.Lang.Object, Request.ICallback
        {
            public void OnCompleted(Response p0)
            {
                Log.Info("RESPONSE", p0.ToString());
            }
        }
    }
}

