import 'package:ecommerce_desktop/providers/auth_provider.dart';
import 'package:ecommerce_desktop/providers/product_provider.dart';
import 'package:ecommerce_desktop/providers/product_review_provider.dart';
import 'package:ecommerce_desktop/providers/product_type_provider.dart';
import 'package:ecommerce_desktop/providers/unit_of_measure_provider.dart';
import 'package:ecommerce_desktop/screens/product_list.dart';
import 'package:ecommerce_desktop/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import 'screens/dashboard_screen.dart';
import 'providers/asset_provider.dart';
import 'providers/category_provider.dart';
import 'providers/user_provider.dart';
import 'providers/usluga_kategorija_provider.dart';
import 'providers/usluga_provider.dart';
import 'providers/frizer_provider.dart';
import 'providers/termin_provider.dart';
import 'services/signalr_service.dart';

/// Globalni kljuc kako bismo mogli prikazati SnackBar (npr. za live notifikaciju)
/// iz bilo kojeg dijela aplikacije, bez obzira na trenutni ekran.
final GlobalKey<ScaffoldMessengerState> rootScaffoldMessengerKey =
    GlobalKey<ScaffoldMessengerState>();

void main() {
  runApp(
    MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_)=> AuthProvider()),
        ChangeNotifierProvider(create: (_)=> ProductProvider()),
        ChangeNotifierProvider(create: (_)=> ProductTypeProvider()),
        ChangeNotifierProvider(create: (_)=> UnitOfMeasureProvider()),
        ChangeNotifierProvider(create: (_)=> AssetProvider()),
        ChangeNotifierProvider(create: (_)=> CategoryProvider()),
        ChangeNotifierProvider(create: (_)=> UserProvider()),
        ChangeNotifierProvider(create: (_)=> ProductReviewProvider()),
        ChangeNotifierProvider(create: (_)=> UslugaKategorijaProvider()),
        ChangeNotifierProvider(create: (_)=> UslugaProvider()),
        ChangeNotifierProvider(create: (_)=> FrizerProvider()),
        ChangeNotifierProvider(create: (_)=> TerminProvider()),
      ],
      child: const MyApp()));
}


class MyApp extends StatelessWidget {
  const MyApp({super.key});

  // This widget is the root of your application.
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      scaffoldMessengerKey: rootScaffoldMessengerKey,
      title: 'Flutter Demo',
      theme: ThemeData(
        // This is the theme of your application.
        //
        // TRY THIS: Try running your application with "flutter run". You'll see
        // the application has a purple toolbar. Then, without quitting the app,
        // try changing the seedColor in the colorScheme below to Colors.green
        // and then invoke "hot reload" (save your changes or press the "hot
        // reload" button in a Flutter-supported IDE, or press "r" if you used
        // the command line to start the app).
        //
        // Notice that the counter didn't reset back to zero; the application
        // state is not lost during the reload. To reset the state, use hot
        // restart instead.
        //
        // This works for code too, not just values: Most code changes can be
        // tested with just a hot reload.
        colorScheme: .fromSeed(seedColor: Colors.blue),
      ),
      home: LoginScreen(),
    );
  }
}

class LoginScreen extends StatelessWidget {
  LoginScreen({super.key});
  final TextEditingController _usernameController = TextEditingController();
  final TextEditingController _passwordController = TextEditingController();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text("Login"),
      ),
      body: Center(
        child: Container(
          constraints: BoxConstraints(maxWidth: 400, maxHeight: 400),
          child: Card(
           
            child: Padding(padding: EdgeInsets.all(16.0),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.spaceAround,
              crossAxisAlignment: CrossAxisAlignment.center,
              children: [
                Image.network(
                    "https://fit.ba/content/763cbb87-718d-4eca-a991-343858daf424",
                    width: 100,
                    height: 100,),
                    TextField(
                      controller: _usernameController,
                      decoration: InputDecoration(
                        labelText: "Username",
                      ),
                    ),
                    SizedBox(height: 16.0,),
                    TextField(
                      controller: _passwordController,
                      obscureText: true,

                      decoration: InputDecoration(
                        labelText: "Password",

                      ),
                    ),
                    SizedBox(height: 16.0,),
                    ElevatedButton(
                      child: Text("Login"),
                      onPressed: () async {

                        AuthProvider authProvider = Provider.of<AuthProvider>(context, listen: false);
                        try {
                          await authProvider.login(_usernameController.text, _passwordController.text);
                          _pokreniSignalR();
                          Navigator.push(context, MaterialPageRoute(builder: (context) => DashboardScreen()));
                        } on Exception catch (e) {
                          alertBox(context, "Error", e.toString());
                        }
                        // Handle login logic here
                        print("Login button pressed");
                      },
                    )
              ],
            ),),
          ),
        ),
      ),
    );
  }
}

/// Pokrece SignalR konekciju nakon uspjesnog logina i postavlja callback koji
/// prikazuje SnackBar cim stigne nova notifikacija uzivo (bez rucnog refresha).
void _pokreniSignalR() {
  SignalRService.instance.onNotifikacija = (notifikacija) {
    final naslov = notifikacija['naslov']?.toString() ?? 'Nova notifikacija';
    final poruka = notifikacija['poruka']?.toString() ?? '';
    rootScaffoldMessengerKey.currentState?.showSnackBar(
      SnackBar(
        content: Text(poruka.isEmpty ? naslov : '$naslov\n$poruka'),
        duration: const Duration(seconds: 4),
      ),
    );
  };
  SignalRService.instance.connect();
}

class MyHomePage extends StatefulWidget {
  const MyHomePage({super.key, required this.title});

  // This widget is the home page of your application. It is stateful, meaning
  // that it has a State object (defined below) that contains fields that affect
  // how it looks.

  // This class is the configuration for the state. It holds the values (in this
  // case the title) provided by the parent (in this case the App widget) and
  // used by the build method of the State. Fields in a Widget subclass are
  // always marked "final".

  final String title;

  @override
  State<MyHomePage> createState() => _MyHomePageState();
}

class _MyHomePageState extends State<MyHomePage> {

  @override
  Widget build(BuildContext context) {
    // This method is rerun every time setState is called, for instance as done
    // by the _incrementCounter method above.
    //
    // The Flutter framework has been optimized to make rerunning build methods
    // fast, so that you can just rebuild anything that needs updating rather
    // than having to individually change instances of widgets.
    return Scaffold(
      appBar: AppBar(
        // TRY THIS: Try changing the color here to a specific color (to
        // Colors.amber, perhaps?) and trigger a hot reload to see the AppBar
        // change color while the other colors stay the same.
        backgroundColor: Theme.of(context).colorScheme.inversePrimary,
        // Here we take the value from the MyHomePage object that was created by
        // the App.build method, and use it to set our appbar title.
        title: Text(widget.title),
      ),
      body: Center(
        // Center is a layout widget. It takes a single child and positions it
        // in the middle of the parent.
        child: Row(children: [
          Container(
            color: Theme.of(context).colorScheme.primary,
            width: 100,
            height: 100,
            margin: const EdgeInsets.all(8.0),
          ),
          Container(
            color: Theme.of(context).colorScheme.secondary,
            width: 100,
            height: 100,
            

            child: Column(
              children: [
                Container(
                  color: Colors.green,
                  width: 50,
                  height: 50,
                 
                ),
                Container(
                  color: Colors.yellow,
                  width: 50,
                  height: 50,
                  margin: const EdgeInsets.all(8.0),
                ),
              ],
            ),
          ),
        ],),
      ),
    );
  }
}
