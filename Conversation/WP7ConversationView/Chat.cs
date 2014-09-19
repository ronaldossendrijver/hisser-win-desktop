using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace WP7ConversationView
{
  public class Chat
  {
    const string ListOfPunc = "?!.;";

    const int NumOfInput = 1;
    const int NumOfResp = 3;


    static string[,] KnowledgeBase = {
			{"WHAT IS YOUR NAME", 
			 "MY NAME IS CHATTERBOT4.",
			 "YOU CAN CALL ME CHATTERBOT4.",
			 "WHY DO YOU WANT TO KNOW MY NAME?"
			},

			{"HI", 
			 "HI THERE!",
			 "HOW ARE YOU?",
			 "HI!"
			},
			
			{"HOW ARE YOU",
			 "I'M DOING FINE!",
			 "I'M DOING WELL AND YOU?",
			 "WHY DO YOU WANT TO KNOW HOW AM I DOING?"
			},

			{"WHO ARE YOU",
			 "I'M AN A.I PROGRAM.",
			 "I THINK THAT YOU KNOW WHO I'M.",
			 "WHY ARE YOU ASKING?"
			},

			{"ARE YOU INTELLIGENT",
			 "YES,OFCORSE.",
			 "WHAT DO YOU THINK?",
			 "ACTUALY,I'M VERY INTELLIENT!"
			},

			{"ARE YOU REAL",
			 "DOES THAT QUESTION REALLY MATERS TO YOU?",
			 "WHAT DO YOU MEAN BY THAT?",
			 "I'M AS REAL AS I CAN BE."
			}
		};

    static bool isPunc(char ch)
    {
      return ListOfPunc.IndexOf(ch) != -1;
    }

    // removes punctuation and redundant
    // spaces from the user's input
    static void cleanString(ref string str)
    {
      string temp = "";
      char prevChar = '#';
      for (int i = 0; i < str.Length; ++i)
      {
        if (str[i] == ' ' && prevChar == ' ')
        {
          continue;
        }
        else if (!isPunc(str[i]))
        {
          temp = string.Concat(temp, str[i]);
        }
        prevChar = str[i];
      }
      str = temp;
    }

    static void preProcessInput(ref string str)
    {
      str = str.ToUpper();
      cleanString(ref str);
    }

    static List<string> findMatch(string str)
    {
      List<string> result = new List<string>(NumOfResp);
      for (int i = 0; i < KnowledgeBase.GetUpperBound(0); ++i)
      {
        // there has been some improvements made in
        // here in order to make the matching process
        // a littlebit more flexible
        if (str.IndexOf(KnowledgeBase[i, 0]) != -1)
        {
          for (int j = NumOfInput; j <= NumOfResp; ++j)
          {
            result.Add(KnowledgeBase[i, j]);
          }
          break;
        }
      }
      return result;
    }

    string sPrevInput;
    string sPrevResponse;
    string sResponse = "";

    public string GetResponse(string sInput)
    {
      sPrevInput = sInput;
      sPrevResponse = sResponse;
      preProcessInput(ref sInput);
      List<string> responses = new List<string>(NumOfResp);
      responses = findMatch(sInput);
      if (sPrevInput.Equals(sInput) && sPrevInput.Length > 0)
      {
        // controling repetitions made by the user
        return "YOU ARE REPEATING YOURSELF.";
      }
      else if (responses.Count == 0)
      {
        // handles the case when the program doesn't understand what the user is talking about	
        return "I'M NOT SURE IF I UNDERSTAND WHAT YOU ARE TALKING ABOUT.";
      }
      else
      {
        Random generator = new Random();
        int nSelection = generator.Next(0, NumOfResp);
        sResponse = responses[nSelection];
        // avoids repeating the same response
        if (sResponse == sPrevResponse)
        {
          responses.RemoveAt(nSelection);
          nSelection = generator.Next(0, NumOfResp - 1);
          sResponse = responses[nSelection];
        }
        return sResponse;
      }
    }
  }
}
