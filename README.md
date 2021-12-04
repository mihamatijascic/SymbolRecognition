# SymbolRecognition
Laboratory exercise from the course Fuzzy, Evolutionary and Neuro-computing at University of Zagreb, Faculty of Electrical Engineering and Computing. 
Task of exercise is to develop application where user can create a dataset of hand-written symbols (or gestures), use these symbols for learning custom neural network and then 
calculate the prediction for newly hand-written symbols.


# Input Gesture Tab
First tab of application is used to input new symbols (or gestures). New symbols are added by defining name in text box beneath "Input new gesture name:" label and pressing Save button. Then user can select symbol in table underneath "Currently saved gestures:" and start to draw symbol in InkCanvas located on the right side. Changes in number of hand-written patterns of symbol can be obsereved in table. To delete symbol, user must select symbol and press "delete" button beneath table or to delete last written pattern of symbol, user can select symbol for which last pattern will be deleted, and press button "delete last pattern".

![image](https://user-images.githubusercontent.com/69255848/144680208-2b8bfd35-40d6-42b0-bf17-fa8e5192683a.png)

# Configure Neural Network Tab
Second tab of application is used to configure neural network. 
* "Number of Representative Points (Input layer):" defines number of input neurons in neural network configuration (30 points, 60 input neurons beacuse of x and y coordinate of each point). 
* "Hidden Layer configuration" defines architecture of neural network between input layer and output layer of neural network, output layer has number of neurons equal to number of symbols. 
* "Iterations:" defines number of iteration which application needs to perform for learninng neural network if total error of neural network on learning datasets (input symbols) does not fall lower than "Precision:". 
* "Algorythm Type:" defines which type of algoritham ("Stohastic", "Batch" or "MiniBatch") will be used for learning neural netwrok. If "MiniBatch" algorithm is used, "MiniBach Size:" field is enabled and user needs to input size of batch
* "Precision:" defines desirable maximal total error of neural network on learning dataset
* "Learning Rate:" defines learning rate of neural network  
After setting up configuration, user can press "Configure" button to configure neural network and start learning process. When learning is finished, performed iterations and final total error is displayed.

![image](https://user-images.githubusercontent.com/69255848/144680326-661b0a74-411a-4d1e-a9b4-e85da88ee321.png)

# Predict Gesture Tab
Last tab of application is used to test neural network with new symbols which are drawn on InkCanvas located on right side of application. Bar chart located on left side of application shows percentages that tell how sure the neural network is that some symbol is drawn on the InkCanvas. 

![image](https://user-images.githubusercontent.com/69255848/144683832-bfc07143-e770-45a4-bb05-8b2c41652150.png)
