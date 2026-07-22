import 'package:json_annotation/json_annotation.dart';

part 'notifikacija.g.dart';

@JsonSerializable()
class Notifikacija {
  final int? id;
  final int? korisnikId;
  final String? naslov;
  final String? poruka;
  final String? tip;
  final bool? procitano;
  final DateTime? createdAt;
  final int? terminId;

  Notifikacija({
    this.id,
    this.korisnikId,
    this.naslov,
    this.poruka,
    this.tip,
    this.procitano,
    this.createdAt,
    this.terminId,
  });

  factory Notifikacija.fromJson(Map<String, dynamic> json) =>
      _$NotifikacijaFromJson(json);

  Map<String, dynamic> toJson() => _$NotifikacijaToJson(this);
}
