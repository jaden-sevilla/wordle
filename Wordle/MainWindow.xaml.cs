using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics.Metrics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.Windows.Threading;
//using static System.Net.Mime.MediaTypeNames;

namespace Wordle
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public string wordle;
		public string wordGuess;
		public List<Button> keyboardList = new List<Button>();
		public List<string> words = new List<string>();
		public List<string> valids = new List<string>();

		public MainWindow()
		{
			InitializeComponent();
			KeyboardList();
			StartGame();
		}

		private void StartGame()
		{
			wordScreen.Visibility = Visibility.Hidden;
			wordScreen.Background = (Brush)(new BrushConverter().ConvertFrom("#B7B7B7"));
			retry.Visibility = Visibility.Hidden;
			wordGuess = null;
			foreach (StackPanel child in FirstStack.Children)
			{
				try
				{
					foreach (TextBox textBox in child.Children)
					{
						textBox.Clear();
						textBox.Background = Brushes.White;
					}
				}
				catch { }
			}

			Brush lightGray = (Brush)(new BrushConverter().ConvertFrom("#99aab5"));
			foreach (StackPanel child in Keyboard.Children)
			{

				foreach (Button button in child.Children)
				{
					button.Background = lightGray;
				}
			}
			A1.Focus();

			string projectRootDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
			string imageDirectory = Path.Combine(projectRootDirectory, "textFiles");
			string imagePath = Path.Combine(imageDirectory, "words.txt");
			using (StreamReader sr = new StreamReader(imagePath))
			{
				while (sr.EndOfStream == false)
				{
					words.Add(sr.ReadLine());
				}
			}

			imagePath = Path.Combine(imageDirectory, "valids.txt");
			using (StreamReader sr = new StreamReader(imagePath))
			{
                while (sr.EndOfStream== false)
                {
					valids.Add(sr.ReadLine());
                }
            }
			Random rand = new Random();
			wordle = words[rand.Next(words.Count)];
		}

		private void CheckWord(StackPanel stackPanel)
		{
			if (valids.Contains(wordGuess.ToLower()) == false)
			{
				NotValid(); 
				return;
			}

			char[] remainingChars = new char[5];
			string[] status = new string[5];
			wordGuess = wordGuess.ToLower();
			for (int i = 0; i < wordle.Length; i++)
			{
				remainingChars[i] = wordle[i];
			}

			for (int i = 0; i < stackPanel.Children.Count; i++)
			{
				TextBox currentTextBox = stackPanel.Children[i] as TextBox;
				if (wordGuess[i] == wordle[i])
				{
					status[i] = "green";
					remainingChars[i] = ' ';
				}
			}
			for (int i = 0; i < stackPanel.Children.Count; i++)
			{
				TextBox currentTextBox = stackPanel.Children[i] as TextBox;
				if (wordGuess[i] != wordle[i])
				{
					for (int j = 0; j < remainingChars.Length; j++)
					{
						if (wordGuess[i] == remainingChars[j] && remainingChars[j] != ' ')
						{
							status[i] = "yellow";
							remainingChars[j] = ' ';
						}
					}
				}
			}

			Brush green = (Brush)(new BrushConverter().ConvertFrom("#6ca965"));
			Brush yellow = (Brush)(new BrushConverter().ConvertFrom("#c8b653"));
			Brush gray = (Brush)(new BrushConverter().ConvertFrom("#787c7f"));

			for (int i = 0; i < stackPanel.Children.Count; i++)
			{
				TextBox currentTextBox = stackPanel.Children[i] as TextBox;
				if (status[i] == "green")
				{
					currentTextBox.Background = green;
					ChangeKeyboard(wordle[i], "green");
				}
				else if (status[i] == "yellow")
				{
					currentTextBox.Background = yellow;
					ChangeKeyboard(wordGuess[i], "yellow");
				}
				else
				{
					currentTextBox.Background = gray;
					ChangeKeyboard(wordGuess[i], "gray");
				}
			}
			CheckWin(status, stackPanel);
		}

		private void ChangeKeyboard(char c, string colour)
		{
			Brush green = (Brush)(new BrushConverter().ConvertFrom("#6ca965"));
			Brush yellow = (Brush)(new BrushConverter().ConvertFrom("#c8b653"));
			Brush gray = (Brush)(new BrushConverter().ConvertFrom("#787c7f"));

			for (int i = 0; i < keyboardList.Count; i++) 
			{
				try
				{
					if (c == char.Parse((keyboardList[i].Content.ToString()).ToLower()))
					{
						if (colour == "green")
						{
							keyboardList[i].Background = green;
						}
						else if (colour == "yellow")
						{
							if (keyboardList[i].Background != green) // reversed for some reason
							{
								keyboardList[i].Background = yellow;
							}
						}
						else if (colour == "gray")
						{
							keyboardList[i].Background = gray;
						}
					}
				}
				catch { }

			}
		}

		private void CheckWin(string[] status, StackPanel stackPanel)
		{
			bool win = true;
			foreach (string colour in status)
			{
				if (colour != "green")
				{
					win = false;
					break;
				}
			}
			if (win == true)
			{
				WinScreen(win);
			}
			else
			{
				TextBox nextRow = GetNextStackTextBox(stackPanel);
				if (nextRow == null)
				{
					WinScreen(false);
				}
				else
				{
					wordGuess = null;
					nextRow.Focus();
				}
			}
		}

		private void NotValid()
		{
			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(1);
			timer.Tick += Timer_Tick;
			timer.Start();
			wordScreen.Visibility = Visibility.Visible;
			wordScreen.Text = $"{wordGuess.ToUpper()} is not valid";
			wordGuess = wordGuess.Substring(0, wordGuess.Length - 1);
		}
		private void Timer_Tick(object sender, EventArgs e)
		{
			wordScreen.Visibility = Visibility.Collapsed;
			((DispatcherTimer)sender).Stop();
		}

		private void WinScreen(bool win)
		{
			if (win == true)
			{
				wordScreen.Background = (Brush)(new BrushConverter().ConvertFrom("#6ca965"));
				wordScreen.Visibility = Visibility.Visible;
				wordScreen.Text = "You win!";
				retry.Visibility = Visibility.Visible;
			}
			else
			{
				wordScreen.Visibility = Visibility.Visible;
				wordScreen.Text = $"Your word was {wordle.ToUpper()}";

				retry.Visibility = Visibility.Visible;
			}
		}
		private void KeyboardList()
		{
			foreach(StackPanel child in Keyboard.Children)
			{
				foreach (Button button in child.Children)
				{
					keyboardList.Add(button);
				}
			}
		}

		private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			TextBox currentTextBox = sender as TextBox;
			if (e.Key == Key.Back)
			{
				if (string.IsNullOrEmpty(currentTextBox.Text))
				{
					TextBox previousTextBox = GetPreviousTextBox(currentTextBox);
					if (previousTextBox != null)
					{
						wordGuess = wordGuess.Substring(0, wordGuess.Length - 1);
						previousTextBox.Focus();
					}
					else
					{
						wordGuess = null;
					}
				}
			}
			else if (string.IsNullOrEmpty(currentTextBox.Text) == false && e.Key == Key.Enter)
			{
				int index = ((StackPanel)currentTextBox.Parent).Children.IndexOf(currentTextBox);
				char text = char.Parse(currentTextBox.Text);
				if (index == ((StackPanel)currentTextBox.Parent).Children.Count - 1)
				{
					if (wordGuess.Length < 5)
					{
						wordGuess += text;
					}
					CheckWord((StackPanel)currentTextBox.Parent);
				}
			}
			else
			{
				if (string.IsNullOrEmpty(currentTextBox.Text) == false)
				{
					TextBox nextTextBox = GetNextTextBox(currentTextBox);
					char text = char.Parse(currentTextBox.Text);
					if (nextTextBox != null)
					{
						wordGuess += text;
						nextTextBox.Focus();
					}
				}
			}
		}
		private TextBox GetNextTextBox(TextBox currentTextBox)
		{
			// Find the index of the current TextBox in the StackPanel
			int index = ((StackPanel)currentTextBox.Parent).Children.IndexOf(currentTextBox);

			// If not the last TextBox, return the next one in the StackPanel
			if (index < ((StackPanel)currentTextBox.Parent).Children.Count - 1)
			{
				return (TextBox)((StackPanel)currentTextBox.Parent).Children[index + 1];
			}

			return null;
		}
		private TextBox GetPreviousTextBox(TextBox currentTextBox)
		{
			int index = ((StackPanel)currentTextBox.Parent).Children.IndexOf(currentTextBox);

			// If it's not the first TextBox, return the one before it
			if (index > 0)
			{
				return (TextBox)((StackPanel)currentTextBox.Parent).Children[index - 1];
			}

			return null;
		}
		private TextBox GetNextStackTextBox(StackPanel stackPanel)
		{
			int index = ((StackPanel)stackPanel.Parent).Children.IndexOf(stackPanel);
			if (index < ((StackPanel)stackPanel.Parent).Children.Count - 1)
			{
				StackPanel newStack = (StackPanel)((StackPanel)stackPanel.Parent).Children[index + 1];

				return (TextBox)newStack.Children[0];
			}
			return null;
		}

		private void retry_Click(object sender, RoutedEventArgs e)
		{
			StartGame();
		}

		private void keyboard_Click(object sender, RoutedEventArgs e)
		{
			Button button = sender as Button;
			bool done = false;

			for (int i = FirstStack.Children.Count - 1; i >= 0; i--)
			{
				if (done == true)
				{
					break;
				}
				StackPanel child = FirstStack.Children[i] as StackPanel;
				for (int j = child.Children.Count - 1; j >= 0; j--)
				{
					TextBox child2 = child.Children[j] as TextBox;
					char letter = char.Parse(button.Content.ToString());

					int index = ((StackPanel)child2.Parent).Children.IndexOf(child2);
					if (string.IsNullOrEmpty(child2.Text) == false)
					{
						if (index < child.Children.Count - 1)
						{
							TextBox nextTextBox = GetNextTextBox(child2);
							nextTextBox.Text = letter.ToString();
							wordGuess += child2.Text;
							if (child2 != null)
							{
								nextTextBox.SelectionStart = 1;
								nextTextBox.Focus();
							}
							done = true;
							break;
						}
						else if (index == child.Children.Count - 1)
						{
							if (wordGuess == null)
							{
								TextBox nextTextBox = GetNextStackTextBox(child);
								nextTextBox.Text = letter.ToString();
								nextTextBox.SelectionStart = 1;
								nextTextBox.Focus();
								done = true;
								break;
							}
							done = true;
							if (wordGuess.Length < 5)
							{
								wordGuess += child2.Text;
							}
							child2.SelectionStart = 1;
							child2.Focus();
							break;
						}
					}
					else if (i == 0 && j == 0)
					{
						child2.Text = letter.ToString();
						child2.SelectionStart = 1;
						child2.Focus();
						done = true;
						break;
					}
				}
			}
		}

		private void enter_Click(object sender, RoutedEventArgs e)
		{
			for (int i = FirstStack.Children.Count - 1; i >= 0; i--)
			{
				StackPanel child = FirstStack.Children[i] as StackPanel;
				for (int j = child.Children.Count - 1; j >= 0; j--)
				{
					TextBox child2 = child.Children[j] as TextBox;
					if (string.IsNullOrEmpty(child2.Text) == false)
					{
						int index = ((StackPanel)child2.Parent).Children.IndexOf(child2);
						if (index == child.Children.Count - 1)
						{
							wordGuess += child2.Text;
							CheckWord((StackPanel)child2.Parent);
							return;
						}
					}
				}
			}
		}

		private void delete_Click(object sender, RoutedEventArgs e)
		{
			for (int i = FirstStack.Children.Count - 1; i >= 0; i--)
			{
				StackPanel child = FirstStack.Children[i] as StackPanel;
				for (int j = child.Children.Count - 1; j >= 0; j--)
				{
					TextBox child2 = child.Children[j] as TextBox;
					if (string.IsNullOrEmpty(child2.Text) == false)
					{
						child2.Text = string.Empty;
						TextBox prevTextBox = GetPreviousTextBox(child2);
						if (prevTextBox != null)
						{
							wordGuess = wordGuess.Substring(0, wordGuess.Length - 1);
							prevTextBox.Focus();
						}
						return;
					}
				}
			}
		}
	}
}