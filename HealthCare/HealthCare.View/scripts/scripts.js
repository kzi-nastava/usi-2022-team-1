function currentTime() {
    let days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"]
    let date = new Date(); 
    let hh = date.getHours();
    let mm = date.getMinutes();
    let ss = date.getSeconds();
    let session = "AM";
  
    if(hh == 0){
        hh = 12;
    }
    if(hh > 12){
        hh = hh - 12;
        session = "PM";
     }
  
     hh = (hh < 10) ? "0" + hh : hh;
     mm = (mm < 10) ? "0" + mm : mm;
     ss = (ss < 10) ? "0" + ss : ss;
      
     let time = hh + ":" + mm + ":" + ss + " " + session;
  
    document.getElementById("clock").innerText = time; 
    document.getElementById("date").innerText = date.toLocaleDateString();
    document.getElementById("year").innerText = days[date.getDay() - 1]; 
    let t = setTimeout(function(){ currentTime() }, 1000);
  }

  function setNameOnCorner(user)
  {
    let smallName = document.getElementById("smallName");
    smallName.innerText = user.name + ' ' + user.surname;
  }