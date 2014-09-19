using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Markup;
using WP7Contrib.View.Controls.Extensions;
using System.Diagnostics;

namespace WP7ConversationView
{
  public partial class MainPage : PhoneApplicationPage
  {

    private MessageCollection messages = new MessageCollection();
    private Chat chatbot = new Chat();
    private Storyboard scrollViewerStoryboard;
    private DoubleAnimation scrollViewerScrollToEndAnim;

    #region VerticalOffset DP

    /// <summary>
    /// VerticalOffset, a private DP used to animate the scrollviewer
    /// </summary>
    private DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset",
      typeof(double), typeof(MainPage), new PropertyMetadata(0.0, OnVerticalOffsetChanged));

    private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      MainPage app = d as MainPage;
      app.OnVerticalOffsetChanged(e);
    }

    private void OnVerticalOffsetChanged(DependencyPropertyChangedEventArgs e)
    {
      ConversationScrollViewer.ScrollToVerticalOffset((double)e.NewValue);
    }

    #endregion

    // Constructor
    public MainPage()
    {
      InitializeComponent();

      messages.Add(new Message()
      {
        Side = MessageSide.You,
        Text = "HI, MY NAME IS ELIZA. WHAT IS ON YOUR MIND?"
      });

      this.DataContext = messages;

      scrollViewerScrollToEndAnim = new DoubleAnimation()
      {
        Duration = TimeSpan.FromSeconds(1),
        EasingFunction = new SineEase()
      };
      Storyboard.SetTarget(scrollViewerScrollToEndAnim, this);
      Storyboard.SetTargetProperty(scrollViewerScrollToEndAnim, new PropertyPath(VerticalOffsetProperty));

      scrollViewerStoryboard = new Storyboard();
      scrollViewerStoryboard.Children.Add(scrollViewerScrollToEndAnim);
      this.Resources.Add("foo", scrollViewerStoryboard);
    }

    private void SendButton_Click(object sender, EventArgs e)
    {
      // loose focus to hide keyboard
      this.Focus();
      messages.Add(new Message()
      {
        Side = MessageSide.Me,
        Text = TextInput.Text
      });
            
      string response = chatbot.GetResponse(TextInput.Text);
      TextInput.Text = "";

      messages.Add(new Message()
      {
        Side = MessageSide.You,
        Text = response
      });
    }

    private void TextInput_GotFocus(object sender, RoutedEventArgs e)
    {
      if (ConversationContentContainer.ActualHeight < ConversationScrollViewer.ActualHeight)
      {
        PaddingRectangle.Show(() => ScrollConvesationToEnd());
      }
      else
      {
        ScrollConvesationToEnd();
      }

      ApplicationBar.IsVisible = true;
    }

    private void ScrollConvesationToEnd()
    {
      scrollViewerScrollToEndAnim.From = ConversationScrollViewer.VerticalOffset;
      scrollViewerScrollToEndAnim.To = ConversationContentContainer.ActualHeight;
      scrollViewerStoryboard.Begin();
    }

    private void TextInput_LostFocus(object sender, RoutedEventArgs e)
    {
      PaddingRectangle.Hide();
      ApplicationBar.IsVisible = false;
      ScrollConvesationToEnd();      
    }

  }
}